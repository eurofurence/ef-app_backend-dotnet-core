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
        ///   * For FragmentType `MapExternal`: An stringified json object.
        ///     * Acceptable properties and their expected value (type):
        ///       * `name` - name of target POI (*string*)
        ///       * `street` - street name (*string*)
        ///       * `house` - house humber (*string*)
        ///       * `zip` - zip code of city (*string*)
        ///       * `city` - city (*string*)
        ///       * `country` - country (*string*)
        ///       * `country-a3` - ISO 3166-1 alpha-3 code for country [http://unstats.un.org/unsd/methods/m49/m49alpha.htm] (*string*)
        ///       * `lat` - latitude (*decimal*)
        ///       * `lon` - longitude (*decimal*)
        ///     * Example:
        ///       * `{ name: "Estrel Hotel Berlin", house: "225", street: "Sonnenallee", zip: "12057", city: "Berlin", country: "Germany", lat: 52.473336, lon: 13.458729 }`
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