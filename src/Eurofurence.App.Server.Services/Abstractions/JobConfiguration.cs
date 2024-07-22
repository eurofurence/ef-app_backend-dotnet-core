using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public class JobConfiguration
    {
        public bool Enabled { get; set; }

        public int SecondsInterval { get; set; }
    }
}
