using System;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventRecord : EntityBase
    {
        public int SourceEventId { get; set; }
        public string Slug { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Abstract { get; set; }
        public Guid ConferenceDayId { get; set; }
        public Guid ConferenceTrackId { get; set; }
        public Guid ConferenceRoomId { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string PanelHosts { get; set; }

        public virtual EventConferenceTrackRecord ConferenceTrack { get; set; }
        public virtual EventConferenceDayRecord ConferenceDay { get; set; }
        public virtual EventConferenceRoomRecord ConferenceRoom { get; set; }
    }


}
