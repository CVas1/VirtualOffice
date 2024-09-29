/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
{
    [CustomEditor(typeof(BuildingTerrainCondition))]
    public class BuildingTerrainConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassDetails"),
                new GUIContent("Building Terrain Clear Grass", "Clear grass details on the terrain at placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassRadius"),
                new GUIContent("Building Terrain Clear Grass Radius", "The radius within which to clear grass details on the terrain."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}