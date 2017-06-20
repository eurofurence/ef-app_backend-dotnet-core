using System;

namespace Eurofurence.App.Domain.Model
{
    public class ObjectId
    {
        public DateTime CreationTime { get; set; }
        public int Increment { get; set; }
        public int Machine { get; set; }
        public short Pid { get; set; }
        public int Timestamp { get; set; }
    }
}