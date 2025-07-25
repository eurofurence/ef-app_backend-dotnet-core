﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Domain.Model.Announcements
{
    [DataContract]
    public class AnnouncementRequest : IDtoTransformable<AnnouncementRecord>
    {

        /// <summary>
        /// When does this announcement start to be va lid?
        /// </summary>
        [DataMember]
        [Required]
        public DateTime ValidFromDateTimeUtc { get; set; }

        /// <summary>
        /// Until when will the announcement be valid?
        /// </summary>
        [DataMember]
        [Required]
        public DateTime ValidUntilDateTimeUtc { get; set; }

        /// <summary>
        /// Type of announcement:
        /// <list type="bullet">
        /// <item>
        /// <term>announcement</term>
        /// <description>regular announcement</description>
        /// </item>
        /// <item>
        /// <term>new</term>
        /// <description>newly created event on schedule</description>
        /// </item>
        /// <item>
        /// <term>deleted</term>
        /// <description>scheduled event has been canceled</description>
        /// </item>
        /// <item>
        /// <term>delay</term>
        /// <description>existing event on schedule will be delayed</description>
        /// </item>
        /// <item>
        /// <term>reschedule</term>
        /// <description>existing event on schedule will move to different time slot</description>
        /// </item>
        /// </list>
        /// </summary>
        [DataMember]
        [Required]
        public string Area { get; set; }

        /// <summary>
        /// Department in which the announcement originated (e.g. "Mobile App").
        /// </summary>
        [DataMember]
        [Required]
        public string Author { get; set; }

        /// <summary>
        /// Title of the announcement.
        /// </summary>
        [DataMember]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Text body of the announcement.
        /// </summary>
        [DataMember]
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// ID of an image to be displayed with the announcement, uploaded via the POST endpoint for images.
        /// </summary>
        [DataMember]
        public Guid? ImageId { get; set; }

        /// <summary>
        /// Optional ids for roles in the identity provider.
        /// If provided, the announcement will only be sent to members of those roles.
        /// </summary>
        [DataMember]
        public string[] Roles { get; set; }
    }
}