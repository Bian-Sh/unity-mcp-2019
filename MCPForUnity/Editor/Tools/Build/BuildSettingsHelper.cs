using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace MCPForUnity.Editor.Tools.Build
{
    public static class BuildSettingsHelper
    {
#if UNITY_2021_2_OR_NEWER
        public static object ReadProperty(string property, NamedBuildTarget namedTarget)
#else
        public static object ReadProperty(string property, BuildTargetGroup namedTarget)
#endif
        {
            switch (property.ToLowerInvariant())
            {
                case "product_name":
                    return new { property, value = PlayerSettings.productName };
                case "company_name":
                    return new { property, value = PlayerSettings.companyName };
                case "version":
                    return new { property, value = PlayerSettings.bundleVersion };
                case "bundle_id":
                    return new { property, value = PlayerSettings.GetApplicationIdentifier(namedTarget) };
                case "scripting_backend":
                    var backend = PlayerSettings.GetScriptingBackend(namedTarget);
                    return new { property, value = backend == ScriptingImplementation.IL2CPP ? "il2cpp" : "mono" };
                case "defines":
#if UNITY_2021_2_OR_NEWER
                    return new { property, value = PlayerSettings.GetScriptingDefineSymbols(namedTarget) };
#else
                    return new { property, value = PlayerSettings.GetScriptingDefineSymbolsForGroup(namedTarget) };
#endif
                case "architecture":
                    var arch = PlayerSettings.GetArchitecture(namedTarget);
                    string archName;
                    switch (arch)
                    {
                        case 0: archName = "x86_64"; break;
                        case 1: archName = "arm64"; break;
                        case 2: archName = "universal"; break;
                        default: archName = "unknown"; break;
                    }
                    return new { property, value = archName, raw = arch };
                default:
                    return null;
            }
        }

#if UNITY_2021_2_OR_NEWER
        public static string WriteProperty(string property, string value, NamedBuildTarget namedTarget)
#else
        public static string WriteProperty(string property, string value, BuildTargetGroup namedTarget)
#endif
        {
            try
            {
                switch (property.ToLowerInvariant())
                {
                    case "product_name":
                        PlayerSettings.productName = value;
                        return null;
                    case "company_name":
                        PlayerSettings.companyName = value;
                        return null;
                    case "version":
                        PlayerSettings.bundleVersion = value;
                        return null;
                    case "bundle_id":
                        PlayerSettings.SetApplicationIdentifier(namedTarget, value);
                        return null;
                    case "scripting_backend":
                        var backendValue = value.ToLowerInvariant();
                        if (backendValue != "il2cpp" && backendValue != "mono")
                            return $"Unknown scripting_backend '{value}'. Valid: mono, il2cpp";
                        var impl = backendValue == "il2cpp"
                            ? ScriptingImplementation.IL2CPP
                            : ScriptingImplementation.Mono2x;
                        PlayerSettings.SetScriptingBackend(namedTarget, impl);
                        return null;
                    case "defines":
#if UNITY_2021_2_OR_NEWER
                        PlayerSettings.SetScriptingDefineSymbols(namedTarget, value);
#else
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(namedTarget, value);
#endif
                        return null;
                    case "architecture":
                        string archValue = value.ToLowerInvariant();
                        int arch;
                        if (archValue == "x86_64" || archValue == "none" || archValue == "default") arch = 0;
                        else if (archValue == "arm64") arch = 1;
                        else if (archValue == "universal") arch = 2;
                        else arch = -1;
                        if (arch < 0)
                            return $"Unknown architecture '{value}'. Valid: x86_64, arm64, universal";
                        PlayerSettings.SetArchitecture(namedTarget, arch);
                        return null;
                    default:
                        return $"Unknown property '{property}'. Valid: product_name, company_name, version, bundle_id, scripting_backend, defines, architecture";
                }
            }
            catch (Exception ex)
            {
                return $"Failed to set {property}: {ex.Message}";
            }
        }

        public static readonly IReadOnlyList<string> ValidProperties = new[]
        {
            "product_name", "company_name", "version", "bundle_id",
            "scripting_backend", "defines", "architecture"
        };
    }
}
