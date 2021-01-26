using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HVR.Editor
{
    [InitializeOnLoad]
    public class PluginCompatibility
    {
        public enum PlatformType
        {
            None,
            Android,
            iOS,
            OSX_x86_64,
            Linux_x86_64,
            Windows_x86_64,
        }

        public struct PluginType
        {
            public PlatformType platformType;
            public bool editorSupport;
            public string editorCPU;
            public string editorOS;
            public PluginImportSettings[] importSettings;
        }

        public struct PluginImportSettings
        {
            public string buildTarget;
            public bool enable;
            public Dictionary<string, string> settings;
        }

        static readonly PluginType[] pluginTypes =
        {
            new PluginType
            {
                platformType = PlatformType.Android,
                editorSupport = false,
                importSettings = new PluginImportSettings[]
                {
                    new PluginImportSettings{
                        buildTarget = "Android",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "ARMv7"}
                        }
                    }
                }
            },
            new PluginType
            {
                platformType = PlatformType.iOS,
                importSettings = new PluginImportSettings[]
                {
                    new PluginImportSettings{
                        buildTarget = "iOS",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", ""},
                            { "CompileFlags", ""},
                            { "FrameworkDependencies", ""}
                        }
                    }
                }
            },
            new PluginType
            {
                platformType = PlatformType.Linux_x86_64,
                editorSupport = true,
                editorCPU = "x86_64",
                editorOS = "Linux",
                importSettings = new PluginImportSettings[]
                {
                    new PluginImportSettings{
                        buildTarget = "Linux",
                        enable = false,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "None"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "Linux64",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "x86_64"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "LinuxUniversal",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "AnyCPU"}
                        }
                    },
                }
            },
            new PluginType
            {
                platformType = PlatformType.OSX_x86_64,
                editorSupport = true,
                editorCPU = "x86_64",
                editorOS = "OSX",
                importSettings = new PluginImportSettings[]
                {
#if UNITY_2017_3_OR_NEWER
                    new PluginImportSettings{
                        buildTarget = "OSXIntel",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "None"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "OSXUniversal",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "x86_64"}
                        }
                    },
#else
                    new PluginImportSettings{
                        buildTarget = "OSXIntel",
                        enable = false,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "None"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "OSXIntel64",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "AnyCPU"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "OSXUniversal",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "x86_64"}
                        }
                    },
#endif
                }
            },
            new PluginType
            {
                platformType = PlatformType.Windows_x86_64,
                editorSupport = true,
                editorCPU = "x86_64",
                editorOS = "Windows",
                importSettings = new PluginImportSettings[]
                {
                    new PluginImportSettings{
                        buildTarget = "Win",
                        enable = false,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "None"}
                        }
                    },
                    new PluginImportSettings{
                        buildTarget = "Win64",
                        enable = true,
                        settings = new Dictionary<string, string>()
                        {
                            { "CPU", "AnyCPU"}
                        }
                    },
                }
            },
        };

        public const string PREFS_PLUGIN_COMPATIBLITY_SESSION_ID = "PREFS_PLUGIN_COMPATIBLITY_SESSION_ID";

        static PluginCompatibility()
        {
            // If the current process id is the same as last time, don't run these checks
            // This reduces some overhead
            int currentID = System.Diagnostics.Process.GetCurrentProcess().Id;

            if (GetSessionID() == currentID)
                return;

            SetSessionID(currentID);

            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                //EnsureCompatiblity();
            }
            else
            {
                // While running in the Unity Editor delayCall is required as
                // ScriptableObjects will have null values for all properties
                EditorApplication.delayCall += EnsureCompatiblity;
            }
        }

        public static void EnsureCompatiblity()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (int i = 0; i < EditorSharedResources.instance.pluginCompatibility.Length; i++)
            {
                PluginReferenceController controller = EditorSharedResources.instance.pluginCompatibility[i];

                if (controller != null)
                {
                    foreach (PluginReferenceController.PluginReference reference in controller.references)
                    {
                        SetCompatibility(reference.guid, reference);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void SetCompatibility(string guid, PluginReferenceController.PluginReference reference)
        {
            string pluginPath = AssetDatabase.GUIDToAssetPath(guid);

            PluginImporter plugin = AssetImporter.GetAtPath(pluginPath) as PluginImporter;

            if (plugin == null)
                return;

            bool requireUpdate = false;

            foreach (PluginType pluginType in pluginTypes)
            {
                if (reference.platformType == pluginType.platformType)
                {
                    string any_exclude_editor = plugin.GetPlatformData("Any", "Exclude Editor");
                    string editor_os = plugin.GetPlatformData("Editor", "OS");
                    string editor_cpu = plugin.GetPlatformData("Editor", "CPU");

                    if (pluginType.editorSupport)
                    {
                        if ((string.IsNullOrEmpty(any_exclude_editor) || any_exclude_editor != "0") ||
                            (string.IsNullOrEmpty(editor_os) || editor_os != pluginType.editorOS) ||
                            (string.IsNullOrEmpty(editor_cpu) || editor_cpu != pluginType.editorCPU))
                        {
                            requireUpdate = true;
                            continue;
                        }
                    }
                    else
                    {
                        if ((string.IsNullOrEmpty(any_exclude_editor) || any_exclude_editor != "1"))
                        {
                            requireUpdate = true;
                            continue;
                        }
                    }

                    foreach (PluginImportSettings pis in pluginType.importSettings)
                    {
                        bool plugin_compatible = plugin.GetCompatibleWithPlatform(pis.buildTarget);
                        string any_exclude = plugin.GetPlatformData("Any", "Exclude " + pis.buildTarget);

                        if (plugin_compatible != pis.enable ||
                            (string.IsNullOrEmpty(any_exclude) || (any_exclude != (pis.enable ? "0" : "1"))))
                        {
                            requireUpdate = true;
                            continue;
                        }

                        foreach (KeyValuePair<string, string> kvp in pis.settings)
                        {
                            string platformData = plugin.GetPlatformData(pis.buildTarget, kvp.Key);

                            if (platformData != kvp.Value)
                            {
                                requireUpdate = true;
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    foreach (PluginImportSettings pis in pluginType.importSettings)
                    {
                        bool compatible = plugin.GetCompatibleWithPlatform(pis.buildTarget);
                        string any_excludeplatform = plugin.GetPlatformData("Any", "Exclude " + pis.buildTarget);
                        string buildtarget_cpu = plugin.GetPlatformData(pis.buildTarget, "CPU");

                        if (compatible != false ||
                            (string.IsNullOrEmpty(any_excludeplatform) || any_excludeplatform != "1") ||
                            (string.IsNullOrEmpty(buildtarget_cpu) || buildtarget_cpu != "None"))
                        {
                            requireUpdate = true;
                            continue;
                        }
                    }
                }
            }

            if (requireUpdate)
            {
#if UNITY_5_5_OR_NEWER
                plugin.ClearSettings();
#endif
                plugin.SetCompatibleWithEditor(false);
                plugin.SetCompatibleWithAnyPlatform(false);

                foreach (PluginType pluginType in pluginTypes)
                {
                    if (reference.platformType == pluginType.platformType)
                    {
                        plugin.SetCompatibleWithEditor(pluginType.editorSupport);

                        if (pluginType.editorSupport)
                        {
                            plugin.SetPlatformData("Any", "Exclude Editor", "0");
                            plugin.SetEditorData("OS", pluginType.editorOS);
                            plugin.SetEditorData("CPU", pluginType.editorCPU);
                            plugin.SetPlatformData("Editor", "OS", pluginType.editorOS);
                            plugin.SetPlatformData("Editor", "CPU", pluginType.editorCPU);
                        }
                        else
                        {
                            plugin.SetPlatformData("Any", "Exclude Editor", "1");
                        }

                        foreach (PluginImportSettings pis in pluginType.importSettings)
                        {
                            plugin.SetCompatibleWithPlatform(pis.buildTarget, pis.enable);
                            plugin.SetPlatformData("Any", "Exclude " + pis.buildTarget, pis.enable ? "0" : "1");

                            foreach (KeyValuePair<string, string> kvp in pis.settings)
                            {
                                plugin.SetPlatformData(pis.buildTarget, kvp.Key, kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (PluginImportSettings pis in pluginType.importSettings)
                        {
                            plugin.SetCompatibleWithPlatform(pis.buildTarget, false);
                            plugin.SetPlatformData("Any", "Exclude " + pis.buildTarget, "1");
                            plugin.SetPlatformData(pis.buildTarget, "CPU", "None");
                        }
                    }

                    AssetDatabase.WriteImportSettingsIfDirty(pluginPath);
                }
            }
        }

        public static int GetSessionID()
        {
            if (EditorPrefs.HasKey(PREFS_PLUGIN_COMPATIBLITY_SESSION_ID))
            {
                int time = EditorPrefs.GetInt(PREFS_PLUGIN_COMPATIBLITY_SESSION_ID);
                return time;
            }

            return -1;
        }

        public static void SetSessionID(int id)
        {
            EditorPrefs.SetInt(PREFS_PLUGIN_COMPATIBLITY_SESSION_ID, id);
        }
    }
}
