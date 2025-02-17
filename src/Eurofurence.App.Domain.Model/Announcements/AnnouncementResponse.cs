using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Announcements;

[DataContract]
public class AnnouncementResponse : AnnouncementRequest
{
    [DataMember] public Guid Id { get; set; }


}