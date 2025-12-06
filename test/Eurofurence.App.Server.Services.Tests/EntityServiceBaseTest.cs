using System;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace Eurofurence.App.Server.Services.Tests
{
    public class EntityServiceBaseTest
    {
        public class TestEntity : EntityBase, IDtoTransformable<ResponseBase>
        {
            public string Title { get; set; }
        }

        [Fact]
        public async Task FindOneAsync_With_NonExistingEntity_ReturnsNull()
        {
            var dbMock = new Mock<AppDbContext>();
            var id = Guid.NewGuid();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            var resultReccord = await entityServiceBase.FindOneAsync(id);

            Assert.Null(resultReccord);
        }

        [Fact]
        public async Task FindOneAsync_With_ExistingEntity_ReturnsEntity()
        {
            var dbMock = new Mock<AppDbContext>();

            var id = Guid.NewGuid();
            var mockEntity = new TestEntity { Id = id };

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([mockEntity]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            var resultReccord = await entityServiceBase.FindOneAsync(mockEntity.Id);

            Assert.Equal(resultReccord.Id, id);
        }

        [Fact]
        public async Task FindOneAsync_With_ExistingEntity_With_OtherId_ReturnsNull()
        {
            var dbMock = new Mock<AppDbContext>();
            var id = Guid.NewGuid();
            var announcement = new TestEntity()
            {
                Id = id,
            };
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([announcement]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            var result = await entityServiceBase.FindOneAsync(new Guid());

            Assert.Null(result);
        }

        [Fact]
        public void FindAll_With_NoEntities_ReturnsEmptyList()
        {
            var dbMock = new Mock<AppDbContext>();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            var result = entityServiceBase.FindAll();

            Assert.Empty(result);
        }

        [Fact]
        public void FindAll_With_Entities_ReturnsList()
        {
            var dbMock = new Mock<AppDbContext>();
            var announcement1 = new TestEntity();
            var announcement2 = new TestEntity();

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([announcement1, announcement2]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            var result = entityServiceBase.FindAll();

            Assert.Equal(result, [announcement1, announcement2]);
        }

        [Fact]
        public void FindAll_By_Expression_By_Id_ReturnsList()
        {
            var dbMock = new Mock<AppDbContext>();
            var announcement1 = new TestEntity();
            var announcement2 = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([announcement1, announcement2]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            var result = entityServiceBase.FindAll(a => a.Id == announcement1.Id);

            Assert.Equal(result, [announcement1]);
        }

        [Fact]
        public void FindAll_By_Expression_By_NonExistingId_ReturnsList()
        {
            var dbMock = new Mock<AppDbContext>();
            var announcement1 = new TestEntity();
            var announcement2 = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([announcement1, announcement2]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            var result = entityServiceBase.FindAll(a => a.Id == Guid.NewGuid());

            Assert.Empty(result);
        }


        [Fact]
        public async Task ReplaceOne_With_ExistingEntity_UpdatesEntity()
        {
            // Arrange
            var dbMock = new Mock<AppDbContext>();
            var announcement1 = new TestEntity()
            {
                Title = "Test"
            };
            var announcement2 = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([announcement1, announcement2]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([new EntityStorageInfoRecord()
            {
                EntityType = "TestEntity"
            }]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            // Act
            announcement1.Title = "Test2";
            await entityServiceBase.ReplaceOneAsync(announcement1);

            // Assert
            dbMock.Verify(db => db.Set<TestEntity>().Update(announcement1), Times.Once);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            var result = await entityServiceBase.FindOneAsync(announcement1.Id);
            Assert.Equal("Test2", result.Title);

        }
        [Fact]
        public void InsertOne_With_NewEntity_InsertsAndTouchesEntity()
        {
            // Arrange
            var dbMock = new Mock<AppDbContext>();
            var announcement1 = new TestEntity()
            {
                Title = "Test"
            };
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([new EntityStorageInfoRecord()
            {
                EntityType = "TestEntity"
            }]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            entityServiceBase.InsertOneAsync(announcement1);

            dbMock.Verify(db => db.Add(announcement1), Times.Once);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
        [Fact]
        public async Task DeleteOne_With_ExistingEntity_SoftDelete_DeletesEntity()
        {
            var dbMock = new Mock<AppDbContext>();
            Guid id = Guid.NewGuid();
            var entity1 = new TestEntity()
            {
                Id = id
            };

            var oldTime = entity1.LastChangeDateTimeUtc;

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([entity1]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([new EntityStorageInfoRecord()
            {
                EntityType = "TestEntity"
            }]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            await entityServiceBase.DeleteOneAsync(entity1.Id);

            Assert.True(entity1.IsDeleted == 1);
            Assert.NotEqual(oldTime, entity1.LastChangeDateTimeUtc);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteOne_With_NonExistingEntity_SoftDelete_DoesNothing()
        {
            var dbMock = new Mock<AppDbContext>();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([new EntityStorageInfoRecord()
            {
                EntityType = "TestEntity"
            }]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            await entityServiceBase.DeleteOneAsync(Guid.NewGuid());

            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task DeleteOne_With_ExistingEntity_HardDelete_DeletesEntity()
        {
            var dbMock = new Mock<AppDbContext>();
            Guid id = Guid.NewGuid();
            var entity1 = new TestEntity()
            {
                Id = id
            };

            var oldTime = entity1.LastChangeDateTimeUtc;

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([entity1]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([new EntityStorageInfoRecord()
            {
                EntityType = "TestEntity"
            }]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            await entityServiceBase.DeleteOneAsync(entity1.Id);

            dbMock.Verify(db => db.Remove(entity1), Times.Once);
        }
    }
}