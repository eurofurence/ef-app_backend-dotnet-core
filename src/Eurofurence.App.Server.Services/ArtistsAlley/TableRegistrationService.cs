using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.ArtistsAlley
{
    public class TableRegistrationService : EntityServiceBase<TableRegistrationRecord, TableRegistrationResponse>,
        ITableRegistrationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly AuthorizationOptions _authorizationOptions;
        private readonly ArtistAlleyOptions _artistAlleyOptions;
        private readonly IImageService _imageService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IPushNotificationChannelManager _pushNotificationChannelManager;
        private readonly IHttpUriSanitizer _uriSanitizer;
        private readonly IHtmlSanitizer _htmlSanitizer;
        private readonly ILogger _logger;

        private readonly Regex _telegramHandleRegex = new Regex("^@?[0-9A-Za-z_]{5,32}$");

        public TableRegistrationService(
            AppDbContext context,
            IStorageServiceFactory storageServiceFactory,
            IOptions<AuthorizationOptions> authorizationOptions,
            IOptions<ArtistAlleyOptions> artistAlleyOptions,
            IPrivateMessageService privateMessageService,
            IImageService imageService,
            IPushNotificationChannelManager pushNotificationChannelManager,
            IHttpUriSanitizer uriSanitizer,
            IHtmlSanitizer htmlSanitizer,
            ILoggerFactory loggerFactory
        ) : base(context, storageServiceFactory)
        {
            _appDbContext = context;
            _authorizationOptions = authorizationOptions.Value;
            _artistAlleyOptions = artistAlleyOptions.Value;
            _privateMessageService = privateMessageService;
            _imageService = imageService;
            _pushNotificationChannelManager = pushNotificationChannelManager;
            _uriSanitizer = uriSanitizer;
            _htmlSanitizer = htmlSanitizer;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IQueryable<TableRegistrationRecord> GetRegistrations(
            TableRegistrationRecord.RegistrationStateEnum? state)
        {
            var records = _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .Where(a => !state.HasValue || a.State == state.Value).AsNoTracking();
            return records;
        }

        public override async Task<TableRegistrationRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);
        }

        private void ValidateRegistrationRequest(TableRegistrationRequest request, Stream imageStream)
        {
            if (!string.IsNullOrWhiteSpace(request.WebsiteUrl))
                if (_uriSanitizer.Sanitize(request.WebsiteUrl) is string sanitizedUrl and not null)
                    request.WebsiteUrl = sanitizedUrl;
                else
                    throw new ArgumentException("Invalid website URL.");

            if (!string.IsNullOrWhiteSpace(request.TelegramHandle) &&
                !_telegramHandleRegex.IsMatch(request.TelegramHandle))
            {
                throw new ArgumentException("Invalid Telegram handle.");
            }

            if (!int.TryParse(request.Location, out var locationInt) || locationInt <= 0)
            {
                throw new ArgumentException("Invalid location, table number must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                throw new ArgumentException("Display name is required.");
            }

            if (imageStream == null)
            {
                throw new ArgumentException("Registrations must include an image.");
            }
        }

        public async Task UpdateTableAsync(ClaimsPrincipal user, Guid id, TableRegistrationRequest request,
            Stream imageStream)
        {
            ValidateRegistrationRequest(request, imageStream);
            string subject = user.GetSubject();

            TableRegistrationRecord record = await _appDbContext
                .TableRegistrations
                .FirstOrDefaultAsync(tr => tr.Id == id);

            if (record == null)
            {
                throw new ArgumentException($"No existing approved table registration found with ID {id}.");
            }

            record.Merge(request);

            ImageRecord image = await _imageService
                .InsertImageAsync($"artistalley:{subject}:{user.GetRegSysIds().FirstOrDefault("none")}", imageStream,
                    true, 1500, 1500);
            record.ImageId = image.Id;

            TableRegistrationRecord.StateChangeRecord stateChange =
                record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Pending,
                    $"Data-Updated-By-Artist: {subject}");
            _appDbContext.StateChangeRecord.Add(stateChange);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request, Stream imageStream)
        {
            ValidateRegistrationRequest(request, imageStream);

            var subject = user.GetSubject();
            var activeRegistrations = await _appDbContext.TableRegistrations
                .Where(x =>
                    x.OwnerUid == subject &&
                    x.State == TableRegistrationRecord.RegistrationStateEnum.Pending)
                .ToListAsync();

            foreach (var registration in activeRegistrations)
            {
                if (registration.ImageId is { } imageId) await _imageService.DeleteOneAsync(imageId);
                await DeleteOneAsync(registration.Id);
            }

            var regId = user.GetRegSysIds().FirstOrDefault("none");

            var image = await _imageService.InsertImageAsync(
                $"artistalley:{subject}:{regId}", imageStream, true, 1500, 1500);

            var record = new TableRegistrationRecord()
            {
                OwnerUid = user.GetSubject(),
                OwnerUsername = user.Identity?.Name,
                CreatedDateTimeUtc = DateTime.UtcNow,
                DisplayName = _htmlSanitizer.Sanitize(request.DisplayName),
                WebsiteUrl = request.WebsiteUrl,
                ShortDescription = _htmlSanitizer.Sanitize(request.ShortDescription),
                TelegramHandle = request.TelegramHandle?.TrimStart('@'),
                Location = request.Location,
                ImageId = image?.Id,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            _appDbContext.TableRegistrations.Add(record);
            await _appDbContext.SaveChangesAsync();

            if (_artistAlleyOptions.SendModeratorAnnouncements != true)
            {
                return;
            }

            var announcementRequest = new AnnouncementRequest
            {
                Title = "New Artist Alley Registration",
                Content = $"{record.OwnerUsername} (Reg# {regId}) registered as {request.DisplayName} at table {request.Location}. Please check their table and approve or reject their request.",
                Area = "announcement",
                Author = "Artist Alley",
                ImageId = image?.Id,
                ValidFromDateTimeUtc = DateTime.UtcNow,
                ValidUntilDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Groups = _authorizationOptions.ArtistAlleyAdmin.Concat(_authorizationOptions.ArtistAlleyModerator)
                    .ToArray()
            };

            await _pushNotificationChannelManager.PushAnnouncementNotificationToGroupsAsync(
                announcementRequest.Transform(), announcementRequest.Groups);
        }

        public async Task ApproveByIdAsync(Guid id, string operatorUid, CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                    && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending,
                cancellationToken: cancellationToken);
            var stateChange = record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Accepted, operatorUid);
            _appDbContext.StateChangeRecord.Add(stateChange);
            record.Touch();
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var message =
                $"Dear {record.OwnerUsername},\n\nWe're happy to inform you that your Artist Alley table registration was accepted as suitable for publication.\n\nYour presence in the Artist Alley along with the text and images you provided has been published on the mobile app!";

            var sendPrivateMessageRequest = new SendPrivateMessageByIdentityRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = record.OwnerUid,
                Subject = "Your table registration was accepted",
                Message = message,
                ToastTitle = "Artist Alley",
                ToastMessage = "Your table registration was accepted"
            };

            await _privateMessageService.SendPrivateMessageAsync(sendPrivateMessageRequest,
                cancellationToken: cancellationToken);
        }

        public async Task RejectByIdAsync(Guid id, string operatorUid, CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                    && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending,
                cancellationToken: cancellationToken);

            var stateChange = record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, operatorUid);
            _appDbContext.StateChangeRecord.Add(stateChange);

            record.Touch();

            var message =
                $"Dear {record.OwnerUsername},\n\nWe're sorry to inform you that your Artist Alley table registration was considered not suitable for publication.\n\nIt's possible that we couldn't visit your table in time, or that your submitted texts/images are not suitable for public display.\n\nFeel free to update and re-submit the table registration.";

            var sendPrivateMessageRequest = new SendPrivateMessageByRegSysRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = record.OwnerUid,
                Subject = "Your table registration was rejected",
                Message = message,
                ToastTitle = "Artist Alley",
                ToastMessage = "Your table registration was rejected"
            };

            await _privateMessageService.SendPrivateMessageAsync(sendPrivateMessageRequest,
                cancellationToken: cancellationToken);

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
        {
            var expiredRegistrations = (await FindAll()
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken))
                .Where(r => (DateTime.UtcNow - r.LastChangeDateTimeUtc).TotalHours >
                            _artistAlleyOptions.ExpirationTimeInHours);

            foreach (var registration in expiredRegistrations)
            {
                var message =
                    $"Dear {registration.OwnerUsername},\n\nYour current Artist Alley registration has expired after {_artistAlleyOptions.ExpirationTimeInHours} hours. If you are still at the table, please submit a new registration.";

                var sendPrivateMessageRequest = new SendPrivateMessageByIdentityRequest()
                {
                    AuthorName = "Artist Alley",
                    RecipientUid = registration.OwnerUid,
                    Subject = "Your table registration has expired",
                    Message = message,
                    ToastTitle = "Artist Alley",
                    ToastMessage = "Your table registration has expired"
                };

                await _privateMessageService.SendPrivateMessageAsync(sendPrivateMessageRequest,
                    cancellationToken: cancellationToken);

                await DeleteOneAsync(registration.Id, cancellationToken);

                _logger.LogInformation(LogEvents.Audit,
                    $"Artist alley registration with the ID {registration.Id} of user {registration.OwnerUsername} expired and was deleted.");
            }
        }

        public async Task<TableRegistrationRecord> GetLatestRegistrationByUidAsync(string uid)
        {
            var request = await _appDbContext.TableRegistrations
                .Include(a => a.Image)
                .AsNoTracking()
                .Where(a => a.OwnerUid == uid)
                .OrderByDescending(a => a.CreatedDateTimeUtc)
                .FirstOrDefaultAsync();

            return request;
        }

        public async Task CheckoutLatestRegistrationByUidAsync(string userUid, CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.TableRegistrations
                .OrderByDescending(a => a.CreatedDateTimeUtc)
                .FirstOrDefaultAsync(a => a.OwnerUid == userUid, cancellationToken: cancellationToken);

            if (record == null)
            {
                throw new ArgumentException("User has not been registered yet.");
            }

            await DeleteOneAsync(record.Id, cancellationToken);
        }

        public override async Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (record.ImageId != null)
            {
                await _imageService.DeleteOneAsync((Guid)record.ImageId, cancellationToken);
            }

            await base.DeleteOneAsync(id, cancellationToken);
        }

        public new async Task<DeltaResponse<TableRegistrationResponse>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<TableRegistrationResponse>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                var changedEntities = await
                    GetRegistrations(TableRegistrationRecord.RegistrationStateEnum.Accepted)
                        .Select(x => x.Transform<TableRegistrationResponse>())
                        .ToListAsync(cancellationToken: cancellationToken);

                response.ChangedEntities = changedEntities.ToArray();
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities =
                    GetRegistrations(TableRegistrationRecord.RegistrationStateEnum.Accepted)
                        .IgnoreQueryFilters()
                        .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                var changedEntities = await
                    GetRegistrations(TableRegistrationRecord.RegistrationStateEnum.Accepted)
                        .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc)
                        .Select(x => x.Transform<TableRegistrationResponse>())
                        .ToListAsync(cancellationToken);

                response.ChangedEntities = changedEntities.ToArray();
                response.DeletedEntities = await entities
                    .AsNoTracking()
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }
    }
}