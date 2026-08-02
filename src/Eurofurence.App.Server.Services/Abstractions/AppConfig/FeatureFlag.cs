#nullable enable
using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.AppConfig
{
    /// <summary>
    /// <para>
    /// Used to define a feature flag for the app following the naming convention described below.
    /// If a flag is provided by backend, its value explicitly defines the feature's state in the app
    /// otherwise the app has to fall back to a working default.
    /// </para>
    /// 
    /// <para>
    /// Each feature flag follows the naming scheme <c>FeatureFlag{Name}{Type}</c>, where its type 
    /// states the default from the app perspective. The rationale behind this is if
    /// <c>FeatureFlagFoobarEnabled</c> is not explicitly set, the app can safely assume the inverse
    /// to be true and disable the feature until the flag is explicitly set by the backend.
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
        public enum FlagType
        {
            /// <summary>
            /// Feature is disabled by default. Flag name: <c>FeatureFlag{Name}Enabled</c>
            /// </summary>
            Enabled,
            /// <summary>
            /// Feature is enabled by default. Flag name: <c>FeatureFlag{Name}Disabled</c>
            /// </summary>
            Disabled,
            /// <summary>
            /// Feature flag default is not defined by backend. Can be used for setting arbitrary
            /// <c>string</c>-y values.
            /// Flag name: <c>FeatureFlag{Name}</c>
            /// </summary>
            Dynamic
        }

        /// <summary>
        /// <para>Type part of the flag naming scheme <c>FeatureFlag{Name}{Type}</c>.</para>
        /// Default value: <c>Dynamic</c>
        /// </summary>
        public FlagType Type { get; init; } = FlagType.Dynamic;

        /// <summary>
        /// <para>Explicit value for the feature flag, explicitly defining the feature's state in the app.
        /// Flags with a value of <c>null</c> will not be forwarded to the frontend.</para>
        /// Default value: <c>null</c>
        /// </summary>
        public string? Value { get; init; }

        public static string GetName(KeyValuePair<string, FeatureFlag> featureFlag)
        {
            var typeSuffix = featureFlag.Value.Type switch
            {
                FlagType.Enabled => "Enabled",
                FlagType.Disabled => "Disabled",
                FlagType.Dynamic => "",
                _ => throw new System.NotImplementedException(),
            };
            return $"FeatureFlag{featureFlag.Key}{typeSuffix}";
        }
    }
}
