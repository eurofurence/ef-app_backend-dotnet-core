using System;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class EnsureEntityIdMatchesAttribute : Attribute
    {
        public EnsureEntityIdMatchesAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }

}
