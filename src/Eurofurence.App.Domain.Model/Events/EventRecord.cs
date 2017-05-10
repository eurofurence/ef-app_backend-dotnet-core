using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventRecord : EntityBase
    {
        [IgnoreDataMember]
        public int SourceEventId { get; set; }

        [DataMember]
        public string Slug { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string SubTitle { get; set; }
        [DataMember]
        public string Abstract { get; set; }
        [DataMember]
        public Guid ConferenceDayId { get; set; }
        [DataMember]
        public Guid ConferenceTrackId { get; set; }
        [DataMember]
        public Guid ConferenceRoomId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public TimeSpan Duration { get; set; }
        [DataMember]
        public TimeSpan StartTime { get; set; }
        [DataMember]
        public TimeSpan EndTime { get; set; }
        [DataMember]
        public DateTime StartDateTimeUtc { get; set; }
        [DataMember]
        public DateTime EndDateTimeUtc { get; set; }
        [DataMember]
        public string PanelHosts { get; set; }
        [DataMember]
        public bool IsDeviatingFromConBook { get; set; }

        [IgnoreDataMember]
        public virtual EventConferenceTrackRecord ConferenceTrack { get; set; }
        [IgnoreDataMember]
        public virtual EventConferenceDayRecord ConferenceDay { get; set; }
        [IgnoreDataMember]
        public virtual EventConferenceRoomRecord ConferenceRoom { get; set; }
    }
}
