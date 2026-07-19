using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net.Http;
using ZXing;
using ZXing.Rendering;
using Passbook.Generator;
using System.Security.Cryptography.X509Certificates;
using Passbook.Generator.Tags;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using System.Security.Claims;
using System.Linq;
using System.Text;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Security.Cryptography;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Passbook.Generator.Fields;
using System.Text.Json.Nodes;
using System.IO;

namespace Eurofurence.App.Server.Services.Passes
{
    public class PassService : IPassService
    {
        private readonly IIdentityService _identityService;
        private readonly IImageService _imageService;
        private readonly GlobalOptions _globalOptions;
        private readonly PassOptions _passOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HybridCache _cache;
        private readonly X509Certificate2 _appleWwdrCertificate;
        private readonly X509Certificate2 _passbookCertificate;
        private readonly PassGenerator _passGenerator;
        private readonly ILogger _logger;

        private static string AvatarHttpClientName = "Eurofurence.App.Server.Services.Passes.AvatarHttpClientName";

        public PassService(
            IIdentityService identityService,
            IImageService imageService,
            IOptions<GlobalOptions> globalOptions,
            IOptions<PassOptions> passOptions,
            IHttpClientFactory httpClientFactory,
            HybridCache cache,
            ILoggerFactory loggerFactory)
        {
            _identityService = identityService;
            _imageService = imageService;
            _globalOptions = globalOptions.Value;
            _passOptions = passOptions.Value;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _passGenerator = new PassGenerator();
            _appleWwdrCertificate = X509Certificate2.CreateFromPem(_passOptions.AppleWwdrX509CertificatePem);
            _passbookCertificate = X509Certificate2.CreateFromPem(_passOptions.PassbookX509CertificatePem, _passOptions.PassbookX509KeyPem);
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public PassFile? GenerateDataMatrixCode(ClaimsIdentity identity)
        {
            var registrationId = _identityService.GetRegistrationsIds(identity).FirstOrDefault();

            if (string.IsNullOrEmpty(registrationId))
                return null;

            BarcodeWriterSvg writer = new() { Format = BarcodeFormat.AZTEC };
            SvgRenderer.SvgImage svgImage = writer.Write(registrationId);

            return new PassFile
            {
                data = Encoding.UTF8.GetBytes(svgImage.Content),
                name = $"convention-pass-{registrationId}.svg",
                mimeType = IPassService.MimeTypeSvg
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

        public async Task<PassFile?> GeneratePkpassAsync(ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            var registrationId = _identityService.GetRegistrationsIds(identity).FirstOrDefault();

            if (string.IsNullOrEmpty(registrationId))
                return null;

            var serialNumber = Convert.ToHexString(
                MD5.HashData(Encoding.UTF8.GetBytes(
                    $"{_globalOptions.ConventionIdentifier}-{registrationId}"
                    )
                )
            );

            PassGeneratorRequest request = new PassGeneratorRequest
            {
                PassTypeIdentifier = _passOptions.PassTypeIdentifier,
                TeamIdentifier = _passOptions.TeamIdentifier,
                AssociatedStoreIdentifiers = [_globalOptions.AppIdITunes],
                SerialNumber = serialNumber,
                Description = $"{_globalOptions.ConventionName} {_globalOptions.ConventionNumber} – {_globalOptions.ConventionTheme}",
                OrganizationName = _globalOptions.ConventionOrganization,
                BackgroundColor = "#005953",
                LabelColor = "#FFFFFF",
                ForegroundColor = "#FFFFFF",
                AppleWWDRCACertificate = _appleWwdrCertificate,
                PassbookCertificate = _passbookCertificate,
                Style = PassStyle.EventTicket,
                ExpirationDate = _globalOptions.ConventionEndDateTime,
                LogoText = $"{_globalOptions.ConventionName} {_globalOptions.ConventionNumber}",
                RelevantDate = _globalOptions.ConventionStartDateTime,
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
                await _cache.GetOrCreateAsync("PassService.Pkpass.Icon3X", async cancel => await _imageService.GetImageContentByImageIdAsync(_passOptions.Icon3XImageId, cancel), cancellationToken: cancellationToken)
            );

            if (identity.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value is var avatarUrl && avatarUrl.StartsWith("https://"))
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient(AvatarHttpClientName);
                    var avatarImage = await httpClient.GetByteArrayAsync(avatarUrl);
                    request.Images.Add(PassbookImage.Thumbnail, avatarImage);
                }
                catch
                {
                    _logger.LogWarning($"Failed to load avatar image at {avatarUrl} for user {identity.Name}");
                }
            }

            request.SemanticTags.Add(new AttendeeName(identity.Name));
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
            request.SemanticTags.Add(new AdmissionLevel("Cookie Connoisseur"));

            request.AddBarcode(BarcodeType.PKBarcodeFormatAztec, registrationId, "UTF-8", registrationId);

            return new PassFile
            {
                data = _passGenerator.Generate(request),
                mimeType = IPassService.MimeTypePkpass,
                name = $"convention-pass-{registrationId}.pkpass"
            };
        }
    }
}