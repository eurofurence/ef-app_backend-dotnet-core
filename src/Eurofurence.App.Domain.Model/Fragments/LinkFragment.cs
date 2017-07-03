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
            MapInternal,
            DealerDetail
        }

        [Required]
        [DataMember]
        public FragmentTypeEnum FragmentType { get; set; }

        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///   * For FragmentType `DealerDetail`: The `Id` of the dealer record the link is referencing to.
        /// </summary>
        [Required]
        [DataMember]
        public string Target { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var f = (LinkFragment) obj;

            return
                f.FragmentType == FragmentType
                && f.Name == Name
                && f.Target == Target;
        }
    }
}