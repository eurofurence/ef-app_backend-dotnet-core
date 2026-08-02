using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.AppConfig
{
    /// <summary>
    /// <para>
    /// Used to define a feature flag for the app following the naming convention described belwo.
    /// If a flag is provided by backend, its value explicitly defines the feature's state in the app
    /// otherwise the app has to fall back to a working default.
    /// </para>
    /// 
    /// <para>
    /// Each feature flag follows the naming scheme <c>FeatureFlag{Name}{InvertedDefault}</c>, where
    /// its default state name is inverted to improve readability from the app perspective. The
    /// rationale behind this is if <c>FeatureFlagFoobarEnabled</c> is not explicitly set, the app
    /// can safely assume the inverse to be true and disable the feature until the flag is explicitly
    /// set by the backend.
    /// </para>
    /// 
    /// Examples for resulting feature flag names:
    /// <list>
    ///     <item>
    ///         <term>FeatureFlagFoobarEnabled</term>
    ///         <description>
    ///             Feature is disabled in app if flag is not provided.
    ///             Value <c>true</c> will enable the feature.
    ///             Value <c>false</c> is the assumed default.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>FeatureFlagFoobarDisabled</term>
    ///         <description>
    ///             Feature is enabled in app if flag is not provided.
    ///             Value <c>true</c> will disable the feature.
    ///             Value <c>false</c> is the assumed default.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>FeatureFlagFoobar</term>
    ///         <description>
    ///             App defaults to state not explicitly known to or defined by the backend.
    ///             Value can be any <c>string</c>-y expression e.g.
    ///             <c>true</c>, <c>621</c> or <c>"foobar"</c>.
    ///         </description>
    ///     </item>
    /// </list>
    /// </summary>
    public class FeatureFlag
    {
        public enum DefaultState
        {
            /// <summary>
            /// Feature is enabled by default. Flag name: <c>FeatureFlag{Name}Disabled</c>
            /// </summary>
            Enabled,
            /// <summary>
            /// Feature is disabled by default. Flag name: <c>FeatureFlag{Name}Enabled</c>
            /// </summary>
            Disabled,
            /// <summary>
            /// Feature flag default is not defined by backend. Can be used for setting arbitrary
            /// <c>string</c>-y values.
            /// Flag name: <c>FeatureFlag{Name}</c>
            /// </summary>
            Undefined
        }

        /// <summary>
        /// Default state part of the feature flag naming scheme <c>FeatureFlag{Name}{Default}</c>.
        /// Default value: <c>Undefined</c>
        /// </summary>
        public DefaultState Default { get; init; } = DefaultState.Undefined;

        /// <summary>
        /// Explicit value for the feature flag, explicitly defining the feature's state in the app.
        /// </summary>
        public string Value { get; init; }

        public static string GetName(KeyValuePair<string, FeatureFlag> featureFlag)
        {
            var suffix = featureFlag.Value.Default switch
            {
                DefaultState.Enabled => "Enabled",
                DefaultState.Disabled => "Disabled",
                DefaultState.Undefined => "",
                _ => throw new System.NotImplementedException(),
            };
            return $"FeatureFlag{featureFlag.Key}{suffix}";
        }
    }
}
