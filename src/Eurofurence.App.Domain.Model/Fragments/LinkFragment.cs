using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Fragments
{
    [DataContract]
    public class LinkFragment
    {
        public enum FragmentTypeEnum
        {
            WebExternal,
            MapExternal,
            MapInternal
        }

        [Required]
        [DataMember]
        public FragmentTypeEnum FragmentType { get; set; }

        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public string Target { get; set; }
    }
}
