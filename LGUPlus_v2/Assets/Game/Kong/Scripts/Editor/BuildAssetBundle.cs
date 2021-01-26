using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildAssetBundle : MonoBehaviour {
    // OSX : BuildTarget.StandaloneOSX
    // Android : BuildTarget.Android
    // iOS : BuildTarget.iOS

    [MenuItem("AssetBundle/Build for Android")]
    static void BuildForAndroid()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    [MenuItem("AssetBundle/Build for iOS")]
    static void BuildForiOS()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.iOS);
    }

    [MenuItem("AssetBundle/Build for OSX")]
    static void BuildForOSX()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
    }
}
