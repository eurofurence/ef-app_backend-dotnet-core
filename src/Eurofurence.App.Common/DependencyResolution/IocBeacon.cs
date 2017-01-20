using System;

namespace Eurofurence.App.Common.DependencyResolution
{
    public class IocBeacon : Attribute
    {
        public enum ScopeEnum
        {
            AlwaysUnique,
            Singleton,
            Transient
        }

        public enum EnvironmentEnum
        {
            Any,
            DesignTimeOnly,
            RunTimeOnly
        }

        public Type TargetType { get; set; }
        public ScopeEnum Scope { get; set; }
        public EnvironmentEnum Environment { get; set; }
    }
}
