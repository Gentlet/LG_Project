using UnityEditor;
using UnityEngine;

namespace HVR.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HvrActor))]
    public class HvrActor_Inspector : UnityEditor.Editor
    {
        private HvrActor targetActor;
        private HvrEditorGUI hvrEditorGUI;

        private float lastInspectorRepaint = -1;
        private float lastAssetTime = -1;
        const float inspectorRepaintTimeOffset = 0.01f;

        private void OnEnable()
        {
            targetActor = (HvrActor)target;
            hvrEditorGUI = new HvrEditorGUI();
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {
            if (hvrEditorGUI != null)
            {
                if (hvrEditorGUI.materialEditor != null)
                    DestroyImmediate(hvrEditorGUI.materialEditor);
            }

            EditorApplication.update -= EditorUpdate;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Inspector_Utils.DrawHeader();

            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Asset", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        hvrEditorGUI.ActorDataMode(targetActor, serializedObject);

                        switch (targetActor.dataMode)
                        {
                            case HvrActor.eDataMode.reference:
                                EditorGUILayout.HelpBox("Reference: Drag and drop a file or folder from your project onto the data slot.", MessageType.None);
                                break;
                            case HvrActor.eDataMode.path:
                                EditorGUILayout.HelpBox("Path: Enter a path to a file/folder located on disk, or a network location.", MessageType.None);
                                break;
                        }

                        switch (targetActor.dataMode)
                        {
                            case HvrActor.eDataMode.reference:
                                hvrEditorGUI.ActorDataReference(targetActor, serializedObject);
                                EditorGUILayout.HelpBox("This data will automatically be included when building this project.\nPrefabs and Android apps have special requirements, please see our documentation for more information", MessageType.Info);
                                break;
                            case HvrActor.eDataMode.path:
                                hvrEditorGUI.ActorDataPath(targetActor, serializedObject);
                                EditorGUILayout.HelpBox("This mode will not copy the data in the path when creating a build.\n'Path' mode is recommended for applications which will be downloading data, or can ensure the data will be found at the path.", MessageType.Warning);
                                break;
                        }
                    }
                    EditorGUI.indentLevel--;

                    if (targetActor.dataMode == HvrActor.eDataMode.path)
                    {
                        EditorGUILayout.LabelField("Streaming", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;

                        int selectedCodecIndex = EditorGUILayout.Popup("Selected Codec", targetActor.GetCodecIndex(targetActor.selectedCodec), targetActor.GetAvailableCodecs());
                        if (selectedCodecIndex >= 0 && selectedCodecIndex < targetActor.GetAvailableCodecs().Length)
                            targetActor.selectedCodec = targetActor.GetAvailableCodecs()[selectedCodecIndex];

                        targetActor.pausePlayWhenCaching = EditorGUILayout.Toggle("Pause if caching", targetActor.pausePlayWhenCaching);

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.HelpBox("These settings will be applied when the asset is created", MessageType.None);
                        targetActor.assetPlay = EditorGUILayout.Toggle("Play", targetActor.assetPlay);
                        targetActor.assetLoop = EditorGUILayout.Toggle("Loop", targetActor.assetLoop);
                        targetActor.assetSeekTime = EditorGUILayout.FloatField("Seek To", targetActor.assetSeekTime);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        hvrEditorGUI.RenderMethod(target, serializedObject);
                        hvrEditorGUI.MaterialField(target, serializedObject);
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        if (hvrEditorGUI.UseLighting(target, serializedObject))
                        {
                            EditorGUI.indentLevel++;
                            hvrEditorGUI.CastShadows(target, serializedObject);
                            hvrEditorGUI.ReceiveShadows(target, serializedObject);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel++;
                {
                    hvrEditorGUI.HvrActorScreenspaceQuad(target, serializedObject);

                    hvrEditorGUI.OcclusionCullingEnabled(target, serializedObject);
                    if (targetActor.occlusionCullingEnabled)
                    {
                        EditorGUI.indentLevel++;
                        {
                            hvrEditorGUI.OcclusionCullingMultipler(target, serializedObject);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            if (targetActor.assetInterface != null)
                hvrEditorGUI.AssetPlaybackBar(targetActor, targetActor.assetInterface, serializedObject);

            if (targetActor.material != null)
                hvrEditorGUI.MaterialEditor(target, targetActor.material, serializedObject);

            if (GUI.changed)
            {
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetActor.gameObject.scene);

                EditorUtility.SetDirty(target);

                SceneView.RepaintAll();
            }
        }

        private void EditorUpdate()
        {
            // Do not repaint the inspector every frame otherwise performance will take a hit. 
            if (Time.realtimeSinceStartup >= lastInspectorRepaint + inspectorRepaintTimeOffset)
            {
                lastInspectorRepaint = Time.realtimeSinceStartup;

                if (targetActor != null &&
                    targetActor.assetInterface != null &&
                    targetActor.assetInterface.GetActualTime() != lastAssetTime)
                {
                    lastAssetTime = targetActor.assetInterface.GetActualTime();

                    // When playing the asset, we need to manually force it to redraw otherwise 
                    // WillRender and PrepareRender won't be called and we stick with the last decoded frame
                    Helper.HvrWorldForceRedraw();
                    Repaint();
                }
            }
        }

        private bool HasFrameBounds()
        {
            HvrActor actor = target as HvrActor;
            return (actor.assetInterface != null);
        }

        private Bounds OnGetFrameBounds()
        {
            HvrActor actor = target as HvrActor;

            if (actor.assetInterface != null)
            {
                Bounds b = actor.assetInterface.GetBounds();
                b.center += actor.transform.position;
                return b;
            }

            return new Bounds(actor.transform.position, Vector3.one * 1f);
        }
    }
}
