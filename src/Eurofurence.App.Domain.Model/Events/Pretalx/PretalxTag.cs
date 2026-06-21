using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the tag model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/tags/operation/tags_retrieve
    /// </summary>
    public class PretalxTag : IEquatable<PretalxTag>
    {
        public int Id { get; init; }
        public string Tag { get; init; }
        public Dictionary<string, string> Description { get; init; }
        public string Color { get; init; }
        public bool IsPublic { get; init; }

        public bool Equals(PretalxTag other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as PretalxTag);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}