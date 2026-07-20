using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public interface IPassService
    {
        /// <summary>
        /// MIME type definition for SVG images.
        /// </summary>
        public const string MimeTypeSvg = "image/svg+xml";

        /// <summary>
        /// MIME type definition for Apple's pkpass format.
        /// </summary>
        public const string MimeTypePkpass = "application/vnd.apple.pkpass";

        /// <summary>
        /// Generates a data matrix code for the user based on their registration as SVG.
        /// </summary>
        /// <param name="identity">ClaimsIdentity for the registration ID of which the code should be generated.</param>
        /// <returns>Pass containing SVG image data and metadata or <c>null</c> if no registration is attached to given identity.</returns>
        public PassFile GenerateDataMatrixCode(ClaimsIdentity identity);

        /// <summary>
        /// Generates a wallet pass conformant to Apple's pkpass format, which can be imported into
        /// e.g. Apple or Google wallet.
        /// </summary>
        /// <param name="identity">ClaimsIdentity the associated registration of which should be used for generating the pass.</param>
        /// <returns>Wallet pass containing the registration data or <c>null</c> if no registration is attached to given identity.</returns>
        public Task<PassFile> GeneratePkpassAsync(ClaimsIdentity identity, CancellationToken cancellationToken = default);
    }
}
