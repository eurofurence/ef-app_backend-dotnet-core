using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Knowledge;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Eurofurence.App.Domain.Model.MongoDb
{
    public static class BsonClassMapping
    {
        private static void DefaultMap<T>()
        {
            BsonClassMap.RegisterClassMap<T>(c => {
                c.AutoMap();
                c.SetIgnoreExtraElements(true);
            });
        }

        public static void Register()
        {
            var pack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            };
            ConventionRegistry.Register("EnumStringConvention", pack, t => true);

            BsonClassMap.RegisterClassMap<EntityBase>(c => {
                c.MapIdField(a => a.Id);
                c.AutoMap();
            });

            DefaultMap<EventRecord>();
            DefaultMap<EventConferenceTrackRecord>();
            DefaultMap<EventConferenceDayRecord>();
            DefaultMap<EventConferenceRoomRecord>();
            DefaultMap<KnowledgeGroupRecord>();
            DefaultMap<KnowledgeEntryRecord>();
        }
    }
}
