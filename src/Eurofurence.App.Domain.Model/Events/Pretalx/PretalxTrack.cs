using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxTrack : IEquatable<PretalxTrack>
    {
        public int Id { get; init; }
        public Dictionary<string, string> Name { get; init; }
        public Dictionary<string, string> Description { get; init; }
        public string Color { get; init; }
        public int? Position { get; init; }
        public bool RequireAccessCode { get; init; }
        public bool AttendeeSignupRequired { get; init; }

        public bool Equals(PretalxTrack other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as PretalxTrack);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}