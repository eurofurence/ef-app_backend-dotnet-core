using MongoDB.Bson.Serialization;

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
            BsonClassMap.RegisterClassMap<EntityBase>(c => {
                c.MapIdField(a => a.Id);
                c.AutoMap();
            });

            DefaultMap<EventRecord>();
            DefaultMap<EventConferenceTrackRecord>();
            DefaultMap<EventConferenceDayRecord>();
            DefaultMap<EventConferenceRoomRecord>();
        }
    }
}
