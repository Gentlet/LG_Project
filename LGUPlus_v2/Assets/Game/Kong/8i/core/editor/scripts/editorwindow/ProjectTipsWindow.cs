using HVR.Android.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HVR.Editor
{
    [InitializeOnLoad]
    public class ProjectTipsWindow : EditorWindow
    {
        private abstract class Check
        {
            protected string key;

            public void Ignore()
            {
                EditorPrefs.SetBool(ignorePrefix + key, true);
            }

            public bool IsIgnored()
            {
                return EditorPrefs.HasKey(ignorePrefix + key);
            }

            public void DeleteIgnore()
            {
                EditorPrefs.DeleteKey(ignorePrefix + key);
            }

            public abstract bool IsValid();

            public abstract void DrawGUI();

            public abstract bool IsFixable();

            public abstract void Fix();
        }

        private class Check_RuntimePlatform : Check
        {
            public Check_RuntimePlatform()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return HvrHelper.Support.IsSupportedRuntimePlatform(Application.platform);
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Unsupported Runtime Platform", MessageType.Error);

                string message = EditorUserBuildSettings.activeBuildTarget.ToString() + " is not a supported runtime platform.";
                message += "\n\nThe 8i Unity Plugin supports...";

                for (int i = 0; i < HvrHelper.Support.supportedRuntimePlatforms.Length; i++)
                {
                    message += "\n- " + HvrHelper.Support.supportedRuntimePlatforms[i].ToString();
                }

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override void Fix()
            {
                throw new NotImplementedException();
            }
        }

        private class Check_RuntimePlatform_GraphicsDeviceType : Check
        {
            public Check_RuntimePlatform_GraphicsDeviceType()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (HvrHelper.Support.IsSupportedRuntimePlatform(Application.platform))
                {
                    return HvrHelper.Support.IsSupportedRuntimePlatformGraphicsDeviceType(Application.platform, SystemInfo.graphicsDeviceType);
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Current application is using an unsupported GraphicsAPI", MessageType.Error);

                string message = SystemInfo.graphicsDeviceType + " is not supported on " + Application.platform;

                message += "\n\n";
                message += "Supported:";

                GraphicsDeviceType[] types = HvrHelper.Support.GetSupportedGraphicsDeviceTypes_FromRuntimePlatform(Application.platform);

                for (int i = 0; i < types.Length; i++)
                {
                    message += "\n - " + types[i].ToString();
                }

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override void Fix()
            {
                throw new NotImplementedException();
            }
        }

        private class Check_Buildtarget : Check
        {
            public Check_Buildtarget()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return HvrHelper.Support.IsSupportedBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Unsupported Build Target", MessageType.Error);

                string message = EditorUserBuildSettings.activeBuildTarget.ToString() + " is not a supported build target";
                message += "\n\nThe 8i Unity Plugin supports...";

                for (int i = 0; i < HvrHelper.Support.supportedBuildTargets.Length; i++)
                {
                    message += "\n- " + HvrHelper.Support.supportedBuildTargets[i].ToString();
                }
                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.StandaloneWindows:
#if !UNITY_2017_3_OR_NEWER
                    case BuildTarget.StandaloneOSXIntel:
                    case BuildTarget.StandaloneOSXIntel64:
#endif
                    case BuildTarget.StandaloneLinux:
                    case BuildTarget.StandaloneLinuxUniversal:
                        return true;
                    default:
                        return false;
                }
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
                {
#if UNITY_5_6_OR_NEWER
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
#else
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
#endif
                }

#if !UNITY_2017_3_OR_NEWER
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64)
                {
#if UNITY_5_6_OR_NEWER
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXIntel64);
#else
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel64);
#endif
                }
#endif

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinuxUniversal)
                {
#if UNITY_5_6_OR_NEWER
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
#else
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneLinux64);
#endif
                }
            }
        }

        private class Check_BuildTarget_GraphicsDeviceType : Check
        {
            public Check_BuildTarget_GraphicsDeviceType()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (HvrHelper.Support.IsSupportedBuildTarget(EditorUserBuildSettings.activeBuildTarget))
                {
                    GraphicsDeviceType[] graphicsDeviceTypes = PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget);

                    if (graphicsDeviceTypes.Length != 0)
                    {
                        GraphicsDeviceType firstGraphicsDeviceType = graphicsDeviceTypes[0];
                        return HvrHelper.Support.IsBuildTargetGraphicsDeviceTypeSupported(EditorUserBuildSettings.activeBuildTarget, firstGraphicsDeviceType);
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("BuildTarget is set to use an unsupported GraphicsAPI", MessageType.Error);

                GraphicsDeviceType[] graphicsDeviceTypes = PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget);
                GraphicsDeviceType firstGraphicsDeviceType = graphicsDeviceTypes[0];

                string message = firstGraphicsDeviceType.ToString() + " is not supported on " + EditorUserBuildSettings.activeBuildTarget;
                message += "\n\n";
                message += "Supported:";

                GraphicsDeviceType[] types = HvrHelper.Support.GetSupportedGraphicsDeviceTypeS_FromBuildTarget(EditorUserBuildSettings.activeBuildTarget);

                for (int i = 0; i < types.Length; i++)
                {
                    message += "\n - " + types[i].ToString();
                }

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override void Fix()
            {
                throw new NotImplementedException();
            }
        }

        private class Check_GraphicsDeviceVersion : Check
        {
            public Check_GraphicsDeviceVersion()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return HvrHelper.Support.IsSupportedGraphicsDeviceVersion(SystemInfo.graphicsDeviceVersion);
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Unsupported Graphics Driver Version", MessageType.Error);

                string message = "Detected: " + SystemInfo.graphicsDeviceVersion;
                message += "\n\nThis driver version is either not supported or is too low for the minimum requirements.";

                message += "\n\nSupported Driver Versions...";

                for (int i = 0; i < HvrHelper.Support.DRIVER_Direct3D_SUPPORTED_VERSIONS.Length; i++)
                {
                    message += "\n" + HvrHelper.Support.DRIVER_Direct3D_SUPPORTED_VERSIONS[i].ToString();
                }

                message += "\nOpenGL (" + HvrHelper.Support.DRIVER_OpenGL_MIN_VERSION_MAJOR + "." + HvrHelper.Support.DRIVER_OpenGL_MIN_VERSION_MINOR + " and greater)";

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override void Fix()
            {
                throw new NotImplementedException();
            }
        }

        private class Check_EditorBuildSettingsSceneList : Check
        {
            public Check_EditorBuildSettingsSceneList()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return EditorBuildSettings.scenes.Length != 0;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Build Settings scene list is empty", MessageType.Error);
                EditorGUILayout.LabelField("Detected that a scene has not been added to the Editor Build Settings scene list.\nIf the current project is built, the HVR data will not be exported.", EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                // Ensure that the current active scene has been saved
                if (SceneManager.GetActiveScene().path == string.Empty)
                {
                    if (!EditorSceneManager.SaveOpenScenes())
                        return;
                }

                List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

                EditorBuildSettingsScene scene = new EditorBuildSettingsScene
                {
                    path = EditorSceneManager.GetActiveScene().path,
                    enabled = true,
                };

                scenes.Add(scene);

                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private class Check_SinglePassStereoRendering : Check
        {
            public Check_SinglePassStereoRendering()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
#if UNITY_5_5_OR_NEWER
                return (PlayerSettings.stereoRenderingPath == StereoRenderingPath.MultiPass);
#else
                return (PlayerSettings.singlePassStereoRendering == false);
#endif
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Incompatible Player Setting", MessageType.Error);
                EditorGUILayout.LabelField("Single-Pass Stereo Rendering is not supported", EditorStyles.helpBox);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
#if UNITY_5_5_OR_NEWER
                PlayerSettings.stereoRenderingPath = StereoRenderingPath.MultiPass;
#else
                PlayerSettings.singlePassStereoRendering = false;
#endif
            }
        }

        private class Check_Android_SplitBinary : Check
        {
            public Check_Android_SplitBinary()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return (PlayerSettings.Android.useAPKExpansionFiles == true);
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Android Split Binary is not enabled in the ProjectSettings. It is strongly recommended that you enable it to avoid size limit of 100MB in Google Play.", MessageType.Warning);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                PlayerSettings.Android.useAPKExpansionFiles = true;
            }
        }

        private class Check_Android_UnpackingSceneFirstInList : Check
        {
            public Check_Android_UnpackingSceneFirstInList()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return AndroidEditorUtilities.IsAndroidUnpackingSceneFirstInSceneList();
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Android Unpacking Scene", MessageType.Error);

                string message = "The Android Unpacking Scene is not the first scene in the project scene list.";
                message += "\nThis scene is required to unpack any HVR data that has been included in the StreamingAssets folder of the project.";
                message += "\nFor more information about this process, see the Android section of the documentation";

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                AndroidEditorUtilities.AddAndroidUnpackingScene();
            }
        }

        private class Check_Android_UnpackingSceneEnabled : Check
        {
            public Check_Android_UnpackingSceneEnabled()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    if (AndroidEditorUtilities.IsAndroidUnpackingSceneFirstInSceneList())
                    {
                        return AndroidEditorUtilities.IsAndroidUnpackingSceneEnabled();
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Android Unpacking Scene is not enabled in the BuildSettings", MessageType.Error);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                AndroidEditorUtilities.EnableAndroidUnpackingScene();
            }
        }

        private class Prompt_AndroindBuildMenu : Check
        {
            public Prompt_AndroindBuildMenu()
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return false;
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Android", MessageType.Info);

                string message = "Android requires a custom build step in order to include HVR data with the build";
                message += "\n\nIn order to include HVR data with your build a custom build step must be used. This step will scan the scenes in your project and copy any required HVR data into the project’s StreamingAssets folder.";
                message += "\n\nThis custom build step can be found within the 8i/Android drop down menu at the top of the Unity Editor window.";

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override void Fix()
            {
                throw new NotImplementedException();
            }
        }

#if UNITY_2017_1_OR_NEWER
        private class Check_Mobile_TierSettingsAndQualitySettings : Check 
        {
            public Check_Mobile_TierSettingsAndQualitySettings() {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    TierSettings tier1 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier1);
                    TierSettings tier2 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier2);
                    TierSettings tier3 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier3);

                    bool useCascadedShadows = tier1.cascadedShadowMaps && tier2.cascadedShadowMaps && tier3.cascadedShadowMaps;


                    bool enabledShadows = true;

                    string[] names = QualitySettings.names;
                    int i = 0;
                    while (i < names.Length) {

                        enabledShadows &= QualitySettings.shadows != ShadowQuality.Disable;
                        if (!enabledShadows)
                            break;
                        QualitySettings.SetQualityLevel(i, false);
                        i++;
                    }

                    return useCascadedShadows && enabledShadows;
                }

                return true;
            }

            public override void DrawGUI()
            {
                EditorGUILayout.HelpBox("Shadowing on Mobile Devices", MessageType.Warning);

                string message = "In order to render correct shadows on mobile devices, the 'Cascaded Shadows' in graphics tier settings must be enabled. ";
                message += "\n\nThis can be fixed via Edit > Project Settings > Graphics > Tier Settings > [Mobile Tab] > Cascaded Shadows.";
                message += "\n\nAlso, in dropdown list of Edit >Project Settings > Quality > [Shadows section] > Shadows, ";
                message += "choose 'Hard and Soft Shadows' for all quality levels.";

                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {
                    TierSettings tier1 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier1);
                    TierSettings tier2 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier2);
                    TierSettings tier3 = EditorGraphicsSettings.GetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                        GraphicsTier.Tier3);

                    tier1.cascadedShadowMaps = true;
                    tier2.cascadedShadowMaps = true;
                    tier3.cascadedShadowMaps = true;

                    EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS, GraphicsTier.Tier1, tier1);
                    EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS, GraphicsTier.Tier2, tier2);
                    EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android | BuildTargetGroup.iOS, GraphicsTier.Tier3, tier3);

                    string[] names = QualitySettings.names;
                    int i = 0;
                    while (i < names.Length) {
                        QualitySettings.shadows = ShadowQuality.All;
                        QualitySettings.SetQualityLevel(i, false);
                        i++;
                    }
                }
            }
        }
#endif

        private const string TITLE = "8i | Project Tips";
        private const string MENU_ITEM = "8i/Project Tips";

        const string ignorePrefix = "hvr.ignore.";

        private static Check[] checks = new Check[]
        {
            new Prompt_AndroindBuildMenu(),
            new Check_RuntimePlatform(),
            new Check_RuntimePlatform_GraphicsDeviceType(),
            new Check_Buildtarget(),
            new Check_BuildTarget_GraphicsDeviceType(),
            new Check_GraphicsDeviceVersion(),
            new Check_EditorBuildSettingsSceneList(),
            new Check_SinglePassStereoRendering(),
            new Check_Android_SplitBinary(),
            new Check_Android_UnpackingSceneFirstInList(),
            new Check_Android_UnpackingSceneEnabled(),
#if UNITY_2017_1_OR_NEWER
            new Check_Mobile_TierSettingsAndQualitySettings(),
#endif            
        };

        static ProjectTipsWindow window;

        Vector2 scrollPosition;

        static ProjectTipsWindow()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        [MenuItem(MENU_ITEM, false, 50)]
        public static void ShowWindow()
        {
            window = GetWindow<ProjectTipsWindow>(true);
            window.minSize = new Vector2(320, 300);
            window.maxSize = new Vector2(320, 800);
            window.titleContent = new GUIContent(TITLE);
        }

        static void Update()
        {
            bool show = false;

            foreach (Check check in checks)
            {
                if (!check.IsIgnored() &&
                    !check.IsValid())
                {
                    show = true;
                }
            }

            if (show)
            {
                ShowWindow();
            }

            EditorApplication.update -= Update;
        }

        public void OnGUI()
        {
            if (EditorSharedResources.instance.aboutWindowLogo)
            {
                var rect = GUILayoutUtility.GetRect(position.width, 80, GUI.skin.box);
                GUI.DrawTexture(rect, EditorSharedResources.instance.aboutWindowLogo, ScaleMode.ScaleAndCrop);
            }

            string aboutText = "This window provides tips to help fix common issues with the 8i Unity Plugin and your project.";
            EditorGUILayout.LabelField(aboutText, EditorStyles.textArea);

            int ignoredCount = 0;
            int fixableCount = 0;
            int invalidNotIgnored = 0;

            for (int i = 0; i < checks.Length; i++)
            {
                Check check = checks[i];

                bool ignored = check.IsIgnored();
                bool valid = check.IsValid();
                bool fixable = check.IsFixable();

                if (!valid && 
                    !ignored &&
                    fixable)
                {
                    fixableCount++;
                }

                if (!valid &&
                    !ignored)
                {
                    invalidNotIgnored++;
                }

                if (ignored)
                {
                    ignoredCount++;
                }
            }

            Rect issuesRect = EditorGUILayout.GetControlRect();
            GUI.Box(new Rect(issuesRect.x - 4, issuesRect.y, issuesRect.width + 8, issuesRect.height), "Tips", EditorStyles.toolbarButton);

            if (invalidNotIgnored > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                {
                    for (int i = 0; i < checks.Length; i++)
                    {
                        Check check = checks[i];

                        if (!check.IsIgnored() &&
                            !check.IsValid())
                        {
                            invalidNotIgnored++;

                            GUILayout.BeginVertical("box");
                            {
                                check.DrawGUI();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    // Aligns buttons to the right
                                    GUILayout.FlexibleSpace();

                                    if (check.IsFixable())
                                    {
                                        if (GUILayout.Button("Fix"))
                                            check.Fix();
                                    }

                                    if (GUILayout.Button("Ignore"))
                                        check.Ignore();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();

            if (invalidNotIgnored == 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("No issues found");

                        if (GUILayout.Button("Close Window"))
                            Close();
                    }
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal("box");
            {
                if (fixableCount > 0)
                {
                    if (GUILayout.Button("Accept All"))
                    {
                        if (EditorUtility.DisplayDialog("Accept All", "Are you sure?", "Yes, Accept All", "Cancel"))
                        {
                            for (int i = 0; i < checks.Length; i++)
                            {
                                Check check = checks[i];

                                if (!check.IsIgnored() &&
                                    !check.IsValid())
                                {
                                    if (check.IsFixable())
                                        check.Fix();
                                }
                            }
                        }
                    }
                }

                if (invalidNotIgnored > 0)
                {
                    if (GUILayout.Button("Ignore All"))
                    {
                        if (EditorUtility.DisplayDialog("Ignore All", "Are you sure?", "Yes, Ignore All", "Cancel"))
                        {
                            for (int i = 0; i < checks.Length; i++)
                            {
                                Check check = checks[i];

                                if (!check.IsIgnored())
                                    check.Ignore();
                            }
                        }
                    }
                }

                if (ignoredCount > 0)
                {
                    if (GUILayout.Button("Show Ignored"))
                    {
                        foreach (Check check in checks)
                            check.DeleteIgnore();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
