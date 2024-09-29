/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
{
    [CustomEditor(typeof(BuildingCollisionCondition))]
    public class BuildingCollisionConditionEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingCollisionCondition Target
        {
            get
            {
                return ((BuildingCollisionCondition)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LayerMask"), new GUIContent("Building Collision Layers",
                "Layers that will be taken into account during collision detection."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreBuildingSurface"), new GUIContent("Ignore Building Surface",
                "Toggle to ignore collision with building surfaces during placement."));

            if (!serializedObject.FindProperty("m_IgnoreBuildingSurface").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingSurfaceTags"), new GUIContent("Building Surface Tag(s)",
                    "Building surface tag(s) required to allows the placement."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Tolerance"), new GUIContent("Building Collision Tolerance",
                "Collision tolerance for allowing the placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), new GUIContent("Show Debugs"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}