using System;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;
using SixLabors.Primitives;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class FursuitBadgeService : IFursuitBadgeService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;

        public FursuitBadgeService(
            ConventionSettings conventionSettings, 
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository
            )
        {
            _conventionSettings = conventionSettings;
            _fursuitBadgeRepository = fursuitBadgeRepository;
        }

        public async Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration)
        {
            byte[] imageBytes = Convert.FromBase64String(registration.ImageContent);

            var image = Image.Load(imageBytes);

            image.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Max,
                Size = new Size(240, 320),
                Sampler = new BicubicResampler()
            });

            var ms = new MemoryStream();
            image.SaveAsJpeg(ms, new JpegEncoderOptions() {IgnoreMetadata = true, Quality = 85});

            bool existingRecord = true;

            var record = await _fursuitBadgeRepository.FindOneAsync(a => a.ExternalReference == registration.BadgeNo.ToString());

            if (record == null)
            {
                existingRecord = false;
                record = new FursuitBadgeRecord();
                record.NewId();
            }

            record.ExternalReference = registration.BadgeNo.ToString();
            record.OwnerUid = $"RegSys:{_conventionSettings.ConventionNumber}:{registration.RegNo}";
            record.Gender = registration.Gender;
            record.Name = registration.Name;
            record.Species = registration.Species;
            record.ImageBytes = ms.ToArray();
            record.Touch();
           

            ms.Dispose();

            if (existingRecord)
                await _fursuitBadgeRepository.ReplaceOneAsync(record);
            else
                await _fursuitBadgeRepository.InsertOneAsync(record);

            return record.Id;
        }

        public async Task<byte[]> GetFursuitBadgeImageAsync(Guid id)
        {
            var content = await _fursuitBadgeRepository.FindOneAsync(id);
            return content.ImageBytes;
        }
    }
}
