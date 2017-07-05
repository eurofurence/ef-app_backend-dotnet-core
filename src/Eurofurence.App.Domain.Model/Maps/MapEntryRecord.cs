using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapEntryRecord
    {
        [DataMember]
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        ///     "X" coordinate of the *center* of a *circular area*, expressed as `[Relative Fraction of Map Image Width]`.
        ///       *  A value of `RelativeX=0.5` indicates it's on the horizontal middle of a map, and results in an absolute X of 1000 on a map that is 2000 pixels wide,
        ///          or 500 on a map that is 1000 pixels wide. 0 would indicate the far left side, where as 100 indicates the far right side of the image.
        /// </summary>
        [DataMember]
        public double RelativeX { get; set; }

        /// <summary>
        ///     "Y" coordinate of the *center* of a *circular area*, expressed as `[Relative Fraction of Map Image Height]`.
        ///      *  A value of `RelativeY=0.5` indicates it's on the vertical middle of a map, and results in an absolute Y of 1000 on a map that is 2000 pixels in height,
        ///         or 500 on a map that is 1000 in height. 0 would indicate the top side, where as 100 indicates the bottom side of the image.
        /// </summary>
        [DataMember]
        public double RelativeY { get; set; }

        /// <summary>
        ///     "Radius" of a *circular area* (the center of which described with RelativeX and RelativeY), expressed as `[Relative Fraction of Map Image Height]`.
        ///      *  A value of `RelativeTapRadius=0.02` indicates that the circle has an absolute tap radius of 20 pixels (and a diameter of 40 pixels) on a map that is 
        ///         1000 pixels in height, or a tap radius of 10 pixels (and a diameter of 20 pixels) on a map that is 500 pixels in height.
        /// </summary>
        [DataMember]
        public double RelativeTapRadius { get; set; }

        [DataMember]
        public LinkFragment Link { get; set; }

        [IgnoreDataMember]
        public virtual MapRecord Map { get; set; }
    }
}