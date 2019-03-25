using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Images;

namespace Eurofurence.App.Server.Services.ArtistsAlley
{
    public class TableRegistrationService : ITableRegistrationService
    {
        private readonly IEntityRepository<TableRegistrationRecord> _tableRegistrationRepository;
        private readonly IImageService _imageService;

        public TableRegistrationService(
            IEntityRepository<TableRegistrationRecord> tableRegistrationRepository,
            IImageService imageService)
        {
            _tableRegistrationRepository = tableRegistrationRepository;
            _imageService = imageService;
        }

        public async Task RegisterTableAsync(string uid, TableRegistrationRequest request)
        {
            byte[] imageBytes = Convert.FromBase64String(request.ImageContent);
            var imageFragment = _imageService.GenerateFragmentFromBytes(imageBytes);

            var record = new TableRegistrationRecord()
            {
                OwnerUid = uid,
                DisplayName = request.DisplayName,
                Merchandise = request.Merchandise,
                ShortDescription = request.ShortDescription,
                Image = imageFragment
            };

            record.NewId();
            record.Touch();

            await _tableRegistrationRepository.InsertOneAsync(record);
        }
    }
}
