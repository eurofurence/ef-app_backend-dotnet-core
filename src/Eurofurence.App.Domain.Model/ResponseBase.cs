using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model;

[DataContract]
public class ResponseBase()
{
    [DataMember]
    public Guid Id { get; init; }

    [DataMember]
    public DateTime LastChangeDateTimeUtc { get; set; }

}