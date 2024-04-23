using Eurofurence.App.Domain.Model;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Eurofurence.App.Tests.Common.InMemoryRepository
{
    public class InMemoryEntityRepositoryTests
    {
        private EntityBase GenerateTestEntity()
        {
            return new EntityBase { Id = Guid.NewGuid(), IsDeleted = 0, LastChangeDateTimeUtc = DateTime.UtcNow };
        }
        private EntityBase[] GenerateTestEntities(int count)
        {
            var result = new EntityBase[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GenerateTestEntity();
            }

            return result;
        }

        //[Fact]
        //public async Task InMemoryEntityRepository_WhenDeleteAllIsCalled_ThenDataSetIsEmpty()
        //{
        //    var _repository = new InMemoryEntityRepository<EntityBase>();

        //    for (int i = 0; i < 5; i++)
        //        await _repository.InsertOneAsync(GenerateTestEntity());

        //    await _repository.DeleteAllAsync();

        //    var results = await _repository.FindAllAsync();

        //    Assert.Empty(results);
        //}

        //[Fact]
        //public async Task InMemoryEntityRepository_WhenDeleteOneIsCalled_ThenOneItemIsRemovedFromDataSet()
        //{
        //    var _repository = new InMemoryEntityRepository<EntityBase>();

        //    var testRecords = GenerateTestEntities(5);
        //    var recordToDelete = testRecords[0];

        //    for (int i = 0; i < testRecords.Length; i++)
        //        await _repository.InsertOneAsync(testRecords[i]);

        //    var resultsBeforeDeletion = await _repository.FindAllAsync();
        //    await _repository.DeleteOneAsync(recordToDelete.Id);
        //    var resultsAfterDeletion = await _repository.FindAllAsync();

        //    Assert.Contains(recordToDelete, resultsBeforeDeletion);
        //    Assert.DoesNotContain(recordToDelete, resultsAfterDeletion);
        //}
    }
}
