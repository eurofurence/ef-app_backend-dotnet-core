using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the room model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/rooms/operation/rooms_retrieve
    /// </summary>
    public class PretalxRoom : IEquatable<PretalxRoom>
    {
        public int Id { get; init; }
        public Dictionary<string, string> Name { get; init; }
        public Dictionary<string, string> Description { get; init; }
        public Guid Uuid { get; init; }
        public int? Capacity { get; init; }
        public int? Position { get; init; }

        public bool Equals(PretalxRoom other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as PretalxRoom);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}