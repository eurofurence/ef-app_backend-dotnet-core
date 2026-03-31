using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Moq.EntityFrameworkCore.Dynamic;
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

            var announcements = GenerateTestEntities(1).First();

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
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
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
        public async Task InsertOne_With_NewEntity_InsertsAndTouchesEntity()
        {
            // Arrange
            var dbMock = new Mock<AppDbContext>();

            var announcement1 = GenerateTestEntities(1).First();

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            await entityServiceBase.InsertOneAsync(announcement1);

            dbMock.Verify(db => db.Add(announcement1), Times.Once);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            Mock.Get(announcement1).Verify(e => e.Touch(), Times.Exactly(2));
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

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic([entity1]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSetDynamic([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

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
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

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

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic([entity1]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSetDynamic([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            await entityServiceBase.DeleteOneAsync(entity1.Id);

            dbMock.Verify(db => db.Remove(entity1), Times.Once);
        }

        [Fact]
        public async Task ReplaceMultipleAsync_With_ExistingEntities_ReplacesMultiple()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);
            SetupDbSetMock(dbMock, entities);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            foreach (var entity in entities)
            {
                entity.Title = Guid.NewGuid().ToString();
            }

            List<TestEntity> replEntities = new List<TestEntity>(entities);
            await entityServiceBase.ReplaceMultipleAsync(replEntities);

            dbMock.Verify(db => db.UpdateRange(replEntities), Times.Once);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ReplaceMultipleAsync_With_ExistingEntites_With_NullArguments_ReplacesNothing()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);
            SetupDbSetMock(dbMock, entities);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            await entityServiceBase.ReplaceMultipleAsync(null);

            dbMock.Verify(db => db.UpdateRange(It.IsAny<IEnumerable<object>>()), Times.Never);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        }

        [Fact]
        public async Task ReplaceMultipleAsync_With_ExistingEntites_With_EmptyList_ReplacesNothing()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);
            SetupDbSetMock(dbMock, entities);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));
            await entityServiceBase.ReplaceMultipleAsync([]);

            dbMock.Verify(db => db.UpdateRange(It.IsAny<IEnumerable<object>>()), Times.Never);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        }

        private static void SetupDbSetMock<T>(Mock<AppDbContext> dbMock, IList<T> entities) where T : class
        {
            dbMock.Setup(db => db.Set<T>()).ReturnsDbSet(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
        }

        [Fact]
        public async Task InsertMultiple_With_NewEntities_InsertsAndTouchesEntities()
        {
            // Arrange
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            var mockedEntities = entities.Select(Mock.Get).ToList();

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            await entityServiceBase.InsertMultipleAsync(entities);

            dbMock.Verify(db => db.AddRangeAsync(entities, It.IsAny<CancellationToken>()), Times.Once);
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

            foreach (var mockedEntity in mockedEntities)
            {
                mockedEntity.Verify(e => e.Touch(), Times.Exactly(2));
            }
        }

        [Fact]
        public async Task DeleteMultiple_With_ExistingEntities_SoftDeletesEntities()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            await entityServiceBase.DeleteMultipleAsync(entities.Select(e => e.Id));

            Assert.All(entities, e => Assert.True(e.IsDeleted == 1));
            Assert.All(entities, e => Mock.Get(e).Verify(x => x.Touch(), Times.Exactly(2)));
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteMultiple_With_ExistingEntities_HardDeletesEntities()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);

            await entityServiceBase.DeleteMultipleAsync(entities.Select(e => e.Id));

            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteAll_With_ExistingEntities_SoftDeletesEntities()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);

            await entityServiceBase.DeleteAllAsync();

            dbMock.Verify(db => db.RemoveRange(It.IsAny<DbSet<TestEntity>>()), Times.Exactly(1));
            dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HasOneAsync_With_ExistingEntities_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasOneAsync(entities[0].Id);
            Assert.True(result);
        }

        [Fact]
        public async Task HasOneAsync_With_NonExistingEntities_ReturnsFalse()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasOneAsync(Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task HasManyAsync_With_ExistingEntities_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync(entities[0].Id, entities[1].Id);
            Assert.True(result);
        }

        [Fact]
        public async Task HasManyAsync_With_Enumerable_With_ExistingEntities_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync(new List<Guid?>() { entities[0].Id, entities[1].Id });
            Assert.True(result);
        }

        [Fact]
        public async Task HasManyAsync_With_EmptyEnumerable_With_ExistingEntities_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync(new List<Guid?>() { });
            Assert.True(result);
        }

        [Fact]
        public async Task HasManyAsync_With_ExistingAndMoreEntities_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(5);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync(entities[0].Id, entities[1].Id);
            Assert.True(result);
        }

        [Fact]
        public async Task HasManyAsync_With_NonExistingEntities_ReturnsFalse()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(0);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task HasManyAsync_With_EmptyList_ReturnsTrue()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(0);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);
            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object), useSoftDelete: false);
            bool result = await entityServiceBase.HasManyAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task ResetStorageDeltaAsync_Should_Remove_Deleted_And_Update_NonDeleted_Entities()
        {
            var dbMock = new Mock<AppDbContext>();
            var entities = GenerateTestEntities(2);

            entities[0].IsDeleted = 1;
            entities[0].LastChangeDateTimeUtc = DateTime.UtcNow;
            entities[1].LastChangeDateTimeUtc = DateTime.UtcNow;
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic(entities);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord()
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>(dbMock.Object, new StorageServiceFactory(dbMock.Object))
            {
                CallBase = true
            };

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));


            //service.Setup(s => s._)

            // Act
            await entityServiceBase.ResetStorageDeltaAsync();

            dbMock.Verify(c => c.Set<TestEntity>().Remove(entities[0]), Times.Once);
            dbMock.Verify(c => c.Set<TestEntity>().Update(entities[1]), Times.Once);
        }



        [Fact]
        public async Task GetDeltaResponseAsync_With_NullSince_ReturnsFullDeltaAndRemovalsDisabled()
        {
            var dbMock = new Mock<AppDbContext>();

            var activeEntity = new Mock<TestEntity>().Object;
            activeEntity.Title = "Active";

            var deletedEntity = new Mock<TestEntity>().Object;
            deletedEntity.Title = "Deleted";
            deletedEntity.IsDeleted = 1;

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic([activeEntity, deletedEntity]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity",
                    DeltaStartDateTimeUtc = new DateTime(2024, 1, 1)
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            var result = await entityServiceBase.GetDeltaResponseAsync();

            Assert.True(result.RemoveAllBeforeInsert);
            Assert.Equal(new DateTime(2024, 1, 1), result.StorageDeltaStartChangeDateTimeUtc);
            Assert.NotNull(result.ChangedEntities);
            Assert.Single(result.ChangedEntities);
            Assert.Equal(activeEntity.Id, result.ChangedEntities[0].Id);
            Assert.Empty(result.DeletedEntities);
        }

        [Fact]
        public async Task GetDeltaResponseAsync_With_SinceAfterDeltaStart_ReturnsIncrementalDelta()
        {
            var dbMock = new Mock<AppDbContext>();

            var changedBeforeSince = new Mock<TestEntity>().Object;
            changedBeforeSince.Title = "Ignored";
            changedBeforeSince.LastChangeDateTimeUtc = new DateTime(2024, 1, 10);

            var changedAfterSince = new Mock<TestEntity>().Object;
            changedAfterSince.Title = "Changed";
            changedAfterSince.LastChangeDateTimeUtc = new DateTime(2024, 1, 20);

            var deletedAfterSince = new Mock<TestEntity>().Object;
            deletedAfterSince.Title = "Deleted";
            deletedAfterSince.IsDeleted = 1;
            deletedAfterSince.LastChangeDateTimeUtc = new DateTime(2024, 1, 20);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic([changedBeforeSince, changedAfterSince, deletedAfterSince]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity",
                    DeltaStartDateTimeUtc = new DateTime(2024, 1, 1)
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            var result = await entityServiceBase.GetDeltaResponseAsync(new DateTime(2024, 1, 15));

            Assert.False(result.RemoveAllBeforeInsert);
            Assert.Equal(new DateTime(2024, 1, 1), result.StorageDeltaStartChangeDateTimeUtc);
            Assert.Single(result.ChangedEntities);
            Assert.Equal(changedAfterSince.Id, result.ChangedEntities[0].Id);
            Assert.Single(result.DeletedEntities);
            Assert.Equal(deletedAfterSince.Id, result.DeletedEntities[0]);
        }

        [Fact]
        public async Task GetDeltaResponseAsync_With_SinceEqualDeltaStart_UsesIncrementalBranch()
        {
            var dbMock = new Mock<AppDbContext>();

            var activeEntity = new Mock<TestEntity>().Object;
            activeEntity.Title = "Active";
            activeEntity.LastChangeDateTimeUtc = new DateTime(2024, 1, 10);

            var deletedEntity = new Mock<TestEntity>().Object;
            deletedEntity.Title = "Deleted";
            deletedEntity.IsDeleted = 1;
            deletedEntity.LastChangeDateTimeUtc = new DateTime(2024, 1, 20);

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSetDynamic([activeEntity, deletedEntity]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity",
                    DeltaStartDateTimeUtc = new DateTime(2024, 1, 1)
                }
            ]);

            var entityServiceBase = new EntityServiceBase<TestEntity, ResponseBase>(dbMock.Object, new StorageServiceFactory(dbMock.Object));

            var result = await entityServiceBase.GetDeltaResponseAsync(new DateTime(2024, 1, 1));

            Assert.False(result.RemoveAllBeforeInsert);
            Assert.Single(result.ChangedEntities);
            Assert.Single(result.DeletedEntities);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_Add_InvokesInsertOneAsync()
        {
            var dbMock = new Mock<AppDbContext>();
            var entity = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>
            {
                CallBase = true
            };

            service.Setup(s => s.InsertOneAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await service.Object.ApplyPatchOperationAsync([
                new PatchOperation<TestEntity>
                {
                    Action = ActionEnum.Add,
                    Entity = entity
                }
            ]);

            service.Verify(s => s.InsertOneAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
            service.Verify(s => s.ReplaceOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.DeleteOneAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_Update_InvokesReplaceOneAsync()
        {
            var dbMock = new Mock<AppDbContext>();
            var entity = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>()
            {
                CallBase = true
            };

            service.Setup(s => s.ReplaceOneAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await service.Object.ApplyPatchOperationAsync([
                new PatchOperation<TestEntity>
                {
                    Action = ActionEnum.Update,
                    Entity = entity
                }
            ]);

            service.Verify(s => s.ReplaceOneAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
            service.Verify(s => s.InsertOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.DeleteOneAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_Delete_InvokesDeleteOneAsync()
        {
            var dbMock = new Mock<AppDbContext>();
            var entity = new TestEntity { Id = Guid.NewGuid() };
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>()
            {
                CallBase = true
            };

            service.Setup(s => s.DeleteOneAsync(entity.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await service.Object.ApplyPatchOperationAsync([
                new PatchOperation<TestEntity>
                {
                    Action = ActionEnum.Delete,
                    Entity = entity
                }
            ]);

            service.Verify(s => s.DeleteOneAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
            service.Verify(s => s.InsertOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.ReplaceOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_MultipleOperations_ProcessesThemInOrder()
        {
            var dbMock = new Mock<AppDbContext>();
            var addEntity = new TestEntity();
            var updateEntity = new TestEntity();
            var deleteEntity = new TestEntity { Id = Guid.NewGuid() };

            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var callOrder = new List<string>();

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>()
            {
                CallBase = true
            };

            service.Setup(s => s.InsertOneAsync(addEntity, It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callOrder.Add("Add");
                    return Task.CompletedTask;
                });

            service.Setup(s => s.ReplaceOneAsync(updateEntity, It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callOrder.Add("Update");
                    return Task.CompletedTask;
                });

            service.Setup(s => s.DeleteOneAsync(deleteEntity.Id, It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callOrder.Add("Delete");
                    return Task.CompletedTask;
                });

            await service.Object.ApplyPatchOperationAsync([
                new PatchOperation<TestEntity> { Action = ActionEnum.Add, Entity = addEntity },
                new PatchOperation<TestEntity> { Action = ActionEnum.Update, Entity = updateEntity },
                new PatchOperation<TestEntity> { Action = ActionEnum.Delete, Entity = deleteEntity }
            ]);

            Assert.Equal(["Add", "Update", "Delete"], callOrder);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_UnknownAction_DoesNothing()
        {
            var dbMock = new Mock<AppDbContext>();
            var entity = new TestEntity();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>()
            {
                CallBase = true
            };

            await service.Object.ApplyPatchOperationAsync([
                new PatchOperation<TestEntity>
                {
                    Action = ActionEnum.NotModified,
                    Entity = entity
                }
            ]);

            service.Verify(s => s.InsertOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.ReplaceOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.DeleteOneAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplyPatchOperationAsync_With_EmptyInput_DoesNothing()
        {
            var dbMock = new Mock<AppDbContext>();
            dbMock.Setup(db => db.Set<TestEntity>()).ReturnsDbSet([]);
            dbMock.Setup(db => db.EntityStorageInfos).ReturnsDbSet([
                new EntityStorageInfoRecord
                {
                    EntityType = "TestEntity"
                }
            ]);

            var service = new Mock<EntityServiceBase<TestEntity, ResponseBase>>()
            {
                CallBase = true
            };

            await service.Object.ApplyPatchOperationAsync([]);

            service.Verify(s => s.InsertOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.ReplaceOneAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            service.Verify(s => s.DeleteOneAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static IList<TestEntity> GenerateTestEntities(int count)
        {
            return new List<TestEntity>(Enumerable.Range(0, count).Select(i => new Mock<TestEntity>()).Select(m => m.Object).Select((e, i) =>
            {
                e.Title = $"Test {i}";
                return e;
            }));
        }
    }
}