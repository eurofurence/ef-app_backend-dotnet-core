using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passbook.Generator;
using Passbook.Generator.Fields;
using Passbook.Generator.Tags;
using ZXing;
using ZXing.Rendering;

namespace Eurofurence.App.Server.Services.Passes
{
    public class PassService : IPassService
    {
        private readonly IIdentityService _identityService;
        private readonly IImageService _imageService;
        private readonly GlobalOptions _globalOptions;
        private readonly PassOptions _passOptions;
        private readonly IPassCertificateProvider _passCertificateProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HybridCache _cache;
        private readonly PassGenerator _passGenerator;
        private readonly ILogger _logger;

        private const double AvatarHttpClientTimeout = 10.0;

        public PassService(
            IIdentityService identityService,
            IImageService imageService,
            IOptions<GlobalOptions> globalOptions,
            IOptions<PassOptions> passOptions,
            IPassCertificateProvider passCertificateProvider,
            IHttpClientFactory httpClientFactory,
            HybridCache cache,
            ILoggerFactory loggerFactory)
        {
            _identityService = identityService;
            _imageService = imageService;
            _globalOptions = globalOptions.Value;
            _passOptions = passOptions.Value;
            _passCertificateProvider = passCertificateProvider;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _passGenerator = new PassGenerator();
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public PassFile GenerateSvg(ClaimsIdentity identity)
        {
            var registrationId = _identityService.GetRegistrationsIds(identity).FirstOrDefault();

            if (string.IsNullOrEmpty(registrationId))
                return null;

            BarcodeWriterSvg writer = new() { Format = BarcodeFormat.AZTEC };
            SvgRenderer.SvgImage svgImage = writer.Write(registrationId);

            return new PassFile
            {
                Data = Encoding.UTF8.GetBytes(svgImage.Content),
                Name = $"convention-pass-{registrationId}.svg",
                MimeType = IPassService.MimeTypeSvg
            };
        }

        // Additional (required) semantic tags that are not (yet) part of the <c>tomasmcguinness/dotnet-passbook</c> library.
        // See: https://developer.apple.com/documentation/walletpasses/creating-an-event-pass-using-semantic-tags
        // See: https://developer.apple.com/documentation/walletpasses/semantictags
        // TODO: Consider forking and extending the library?

        /// <summary>
        /// The level of admission the ticket provides, such as general admission, VIP, and so
        /// forth. Use this key for any type of event ticket.
        /// </summary>
        /// <param name="value">
        /// Admission level as reflected on badge/lanyard (e.g. Attendee, Staff, Director, Sponsor, Supersponsor)
        /// </param>
        private class AdmissionLevel(string value) : SemanticTagBaseValue("admissionLevel", value);

        /// <summary>
        /// The name of the person the ticket grants admission to. Use this key for any type of event ticket.
        /// </summary>
        /// <param name="value">
        /// Name of the attendee as printed on their badge.
        /// </param>
        private class AttendeeName(string value) : SemanticTagBaseValue("attendeeName", value);

        /// <summary>
        /// The name of the city or hosting region of the venue. Use this key for any type of event ticket.
        /// </summary>
        /// <param name="value">
        /// Name of the city (e.g. "Hamburg, Germany").
        /// </param>
        private class VenueRegionName(string value) : SemanticTagBaseValue("venueRegionName", value);

        public async Task<PassFile> GeneratePkpassAsync(ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            var registrationId = _identityService.GetRegistrationsIds(identity).FirstOrDefault();

            if (string.IsNullOrEmpty(registrationId))
                return null;

            var serialNumber = $"{_globalOptions.ConventionIdentifier}-{registrationId}";

            var request = new PassGeneratorRequest
            {
                PassTypeIdentifier = _passOptions.PassTypeIdentifier,
                TeamIdentifier = _passOptions.TeamIdentifier,
                AssociatedStoreIdentifiers = [_globalOptions.AppIdITunes],
                SerialNumber = serialNumber,
                Description = $"{_globalOptions.ConventionName} {_globalOptions.ConventionNumber} – {_globalOptions.ConventionTheme}",
                OrganizationName = _globalOptions.ConventionOrganization,
                BackgroundColor = _passOptions.BackgroundColor,
                LabelColor = _passOptions.LabelColor,
                ForegroundColor = _passOptions.ForegroundColor,
                AppleWWDRCACertificate = _passCertificateProvider.AppleWwdrCertificate,
                PassbookCertificate = _passCertificateProvider.PassbookCertificate,
                Style = PassStyle.EventTicket,
                ExpirationDate = _globalOptions.ConventionEndDateTime,
                LogoText = $"{_globalOptions.ConventionName} {_globalOptions.ConventionNumber}",
                SharingProhibited = true
                // Ignore since only one relevant date possible, but the pass should be visible whole con.
                //RelevantDate = _globalOptions.ConventionStartDateTime,
            };

            request.RelevantLocations.Add(new RelevantLocation
            {
                Latitude = _globalOptions.ConventionVenueLocationLatitude,
                Longitude = _globalOptions.ConventionVenueLocationLongitude
            });

            request.AddPrimaryField(new StandardField
            {
                Key = "eventName",
                Label = "Event",
                Value = $"{_globalOptions.ConventionIdentifier} – {_globalOptions.ConventionTheme}",
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });

            request.AddHeaderField(new StandardField
            {
                Key = "badge-number",
                Label = "Badge#",
                Value = registrationId,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });

            request.AddSecondaryField(new StandardField
            {
                Key = "ticket",
                Label = "Description",
                Value = $"Welcome to {_globalOptions.ConventionName} {_globalOptions.ConventionNumber}, {identity.Name}!",
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });

            request.AddAuxiliaryField(new StandardField
            {
                Key = "opening",
                Label = "Opening",
                Value = _globalOptions.ConventionStartDateTime.ToString("yyyy-MM-dd HH:mm"),
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddAuxiliaryField(new StandardField
            {
                Key = "closing",
                Label = "Closing",
                Value = _globalOptions.ConventionEndDateTime.ToString("yyyy-MM-dd HH:mm"),
                TextAlignment = FieldTextAlignment.PKTextAlignmentRight
            });

            request.AddBackField(new StandardField
            {
                Key = "attendee",
                Label = "Nickname",
                Value = identity.Name,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddBackField(new StandardField
            {
                Key = "registration-id",
                Label = "Badge Number",
                Value = registrationId,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddBackField(new StandardField
            {
                Key = "organizer",
                Label = "Organizer",
                Value = _globalOptions.ConventionOrganization,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddBackField(new StandardField
            {
                Key = "website",
                Label = "Website",
                Value = _globalOptions.ConventionWebsite,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddBackField(new StandardField
            {
                Key = "contact",
                Label = "Contact",
                Value = _globalOptions.ConventionContact,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });
            request.AddBackField(new StandardField
            {
                Key = "information",
                Label = "Information",
                Value = _passOptions.Information,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            });

            request.Images.Add(PassbookImage.Icon,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Icon", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.IconImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Icon2X,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Icon2X", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.Icon2XImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Icon3X,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Icon3X", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.Icon3XImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Logo,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Logo", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.LogoImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Logo2X,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Logo2X", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.Logo2XImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Logo3X,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Logo3X", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.Logo3XImageId, cancel), cancellationToken: cancellationToken)
            );
            request.Images.Add(PassbookImage.Background,
                await _cache.GetOrCreateAsync("PassService.Pkpass.Background", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.BackgroundImageId, cancel), cancellationToken: cancellationToken)
            );

            if (identity.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value is { } avatarUrl && avatarUrl.StartsWith("https://"))
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(AvatarHttpClientTimeout);
                    var avatarImage = await httpClient.GetByteArrayAsync(avatarUrl, cancellationToken);
                    if (avatarImage is not null)
                    {
                        request.Images.Add(PassbookImage.Thumbnail, avatarImage);
                    }
                }
                catch
                {
                    _logger.LogWarning($"Failed to load avatar image at {avatarUrl} for user {identity.Name}");
                }
            }

            request.SemanticTags.Add(new EventType(EventTypes.PKEventTypeConvention));
            request.SemanticTags.Add(new EventName($"{_globalOptions.ConventionName} {_globalOptions.ConventionNumber}"));
            request.SemanticTags.Add(new EventStartDate(_globalOptions.ConventionStartDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture)));
            request.SemanticTags.Add(new EventEndDate(_globalOptions.ConventionEndDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture)));
            request.SemanticTags.Add(new VenueName(_globalOptions.ConventionVenueName));
            request.SemanticTags.Add(new VenueRegionName(_globalOptions.ConventionVenueRegion));
            request.SemanticTags.Add(new VenueRoom(_passOptions.VenueRoom));
            request.SemanticTags.Add(new VenueLocation(
                _globalOptions.ConventionVenueLocationLatitude,
                _globalOptions.ConventionVenueLocationLongitude
                )
            );

            request.SemanticTags.Add(new AttendeeName(identity.Name));
            // TODO: Get Admission Level (attendee/staff/director & regular/sponsor/super sponsor)
            // from Claims and Registration System
            //request.SemanticTags.Add(new AdmissionLevel("Cookie Connoisseur"));

            request.AddBarcode(BarcodeType.PKBarcodeFormatAztec, registrationId, "UTF-8", registrationId);

            return new PassFile
            {
                Data = _passGenerator.Generate(request),
                MimeType = IPassService.MimeTypePkpass,
                Name = $"convention-pass-{registrationId}.pkpass"
            };
        }
    }
}