using System;
using Eurofurence.App.Domain.Model.Fragments;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapEntryRequest
    {
        public Guid Id { get; set; }
        /// <summary>
        ///     "X" coordinate of the *center* of a *circular area*, expressed in pixels.
        /// </summary>
        [DataMember]
        public int X { get; set; }

        /// <summary>
        ///     "Y" coordinate of the *center* of a *circular area*, expressed in pixels.
        /// </summary>
        [DataMember]
        public int Y { get; set; }

        /// <summary>
        ///     "Radius" of a *circular area* (the center of which described with X and Y), expressed in pixels.
        /// </summary>
        [DataMember]
        public int TapRadius { get; set; }

        [DataMember]
        public List<LinkFragment> Links { get; set; } = new();
    }
}
