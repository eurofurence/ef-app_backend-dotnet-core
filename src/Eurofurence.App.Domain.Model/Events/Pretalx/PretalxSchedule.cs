using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the schedule model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/schedules/operation/schedules_retrieve
    /// </summary>
    public class PretalxSchedule<SlotType> where SlotType : PretalxSlot
    {
        public int Id { get; init; }
        public string Version { get; init; }
        public DateTime? Published { get; init; }
        public Dictionary<string, string> Comment { get; init; }
        public SlotType[] Slots { get; init; }
    }
}
