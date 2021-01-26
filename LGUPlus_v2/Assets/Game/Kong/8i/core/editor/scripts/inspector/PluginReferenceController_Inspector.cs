using HVR.Editor;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HVR.Editor
{
    [CustomEditor(typeof(PluginReferenceController))]
    [CanEditMultipleObjects]
    public class PluginReferenceController_Inspector : UnityEditor.Editor
    {
        PluginReferenceController self { get { return (PluginReferenceController)target; } }

        private ReorderableList reorderableList;

        private void OnEnable()
        {
            if (self.references == null)
                self.references = new List<PluginReferenceController.PluginReference>();

            reorderableList = new ReorderableList(self.references, typeof(PluginReferenceController.PluginReference), true, true, true, true);

            // Add listeners to draw events
            reorderableList.drawHeaderCallback += DrawHeader;
            reorderableList.drawElementCallback += DrawElement;

            reorderableList.elementHeightCallback = (index) =>
            {
                Repaint();
                return EditorGUIUtility.singleLineHeight * 5;
            };

            reorderableList.onAddCallback += AddItem;
            reorderableList.onRemoveCallback += RemoveItem;
        }

        private void OnDisable()
        {
            // Make sure we don't get memory leaks etc.
            reorderableList.drawHeaderCallback -= DrawHeader;
            reorderableList.drawElementCallback -= DrawElement;

            reorderableList.onAddCallback -= AddItem;
            reorderableList.onRemoveCallback -= RemoveItem;
        }

        public override void OnInspectorGUI()
        {
            // Actually draw the list in the inspector
            reorderableList.DoLayoutList();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Update"))
            {
                EditorUtility.SetDirty(target);

                PluginCompatibility.EnsureCompatiblity();
            }
        }

        /// <summary>
        /// Draws the header of the list
        /// </summary>
        /// <param name="rect"></param>
        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Data");
        }

        /// <summary>
        /// Draws one element of the list (ListItemExample)
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="active"></param>
        /// <param name="focused"></param>
        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            PluginReferenceController.PluginReference item = self.references[index];

            EditorGUI.BeginChangeCheck();

            float o = 0;

            float lx = rect.x;

            item.guid = HvrEditorGUI.DrawObjectSlot(new Rect(rect.x, rect.y + o, rect.width, EditorGUIUtility.singleLineHeight), "Plugin File", item.guid);
            o += EditorGUIUtility.singleLineHeight;

            string pluginPath = AssetDatabase.GUIDToAssetPath(item.guid);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(new Rect(lx, rect.y + o, rect.width, EditorGUIUtility.singleLineHeight), "Path", pluginPath);
            EditorGUI.EndDisabledGroup();
            o += EditorGUIUtility.singleLineHeight;

            item.platformType = (PluginCompatibility.PlatformType)EditorGUI.EnumPopup(new Rect(lx, rect.y + o, rect.width, EditorGUIUtility.singleLineHeight), "Platform Type", item.platformType);
            o += EditorGUIUtility.singleLineHeight;

            if (EditorGUI.EndChangeCheck())
            {
                self.references[index] = item;
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void AddItem(ReorderableList list)
        {
            self.references.Add(new PluginReferenceController.PluginReference());

            EditorUtility.SetDirty(target);
        }

        private void RemoveItem(ReorderableList list)
        {
            self.references.RemoveAt(list.index);

            EditorUtility.SetDirty(target);
        }
    }
}
