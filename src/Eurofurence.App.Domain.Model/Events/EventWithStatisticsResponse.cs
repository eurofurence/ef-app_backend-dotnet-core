using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventWithStatisticsResponse : EventResponse
    {
        public List<EventFavoriteStatisticsResponse> FavoriteStatistics { get; set; } = [];
    }
}