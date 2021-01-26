using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HVR.Editor
{
    public class PluginReferenceController : ScriptableObject
    {
        [Serializable]
        public struct PluginReference
        {
            public string guid;
            public PluginCompatibility.PlatformType platformType;
        }

        public List<PluginReference> references;
    }
}
