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

        [Required]
        [DataMember]
        public string Target { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            LinkFragment f = (LinkFragment)obj;

            return
                (f.FragmentType == this.FragmentType)
                && (f.Name == this.Name)
                && (f.Target == this.Target);            
        }
    }
}
