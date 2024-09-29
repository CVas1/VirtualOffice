/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Collision Condition",
        "This condition checks if the Building Part avoids collisions with undesired colliders during the preview state.\n\n" +
        "You can find more information on the Building Collision Condition component in the documentation.", 2)]
    public class BuildingCollisionCondition : BuildingCondition
    {
        #region Fields

        [SerializeField] LayerMask m_LayerMask = 1 << 0;

        [SerializeField] [Range(0f, 10f)] float m_Tolerance = 0.9f;

        [SerializeField] bool m_IgnoreBuildingSurface = true;

        [SerializeField] string[] m_BuildingSurfaceTags;

        [SerializeField] bool m_ShowDebugs = false;
        
        [SerializeField] bool m_ShowGizmos = true;

        #endregion

        #region Unity Methods

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.Selection.gameObjects.Length > 6)
            {
                return;
            }
#endif

            if (GetBuildingPart == null)
            {
                return;
            }

            Gizmos.matrix = GetBuildingPart.transform.localToWorldMatrix;

            bool canPlacing = CheckPlacingCondition();

            Gizmos.color = (canPlacing ? Color.cyan : Color.red) / 2f;
            Gizmos.DrawCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);

            Gizmos.color = (canPlacing ? Color.cyan : Color.red);
            Gizmos.DrawWireCube(GetBuildingPart.GetModelSettings.ModelBounds.center,
                1.001f * m_Tolerance * GetBuildingPart.GetModelSettings.ModelBounds.size);
        }

        #endregion

        #region Internal Methods

        public override bool CheckPlacingCondition()
        {
#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
    if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
#else
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
#endif
            {
                return true;
            }
#endif

            Bounds worldCollisionBounds = GetBuildingPart.transform.GetWorldBounds(GetBuildingPart.GetModelSettings.ModelBounds);

            Collider[] colliders = PhysicsExtension.GetNeighborsType<Collider>(worldCollisionBounds.center,
                    worldCollisionBounds.extents * m_Tolerance, GetBuildingPart.transform.rotation, m_LayerMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !GetBuildingPart.Colliders.Contains(colliders[i]))
                {
                    BuildingCollisionSurface buildingCollisionSurface = colliders[i].GetComponent<BuildingCollisionSurface>();

                    if (m_ShowDebugs)
                    {
                        Debug.LogWarning("<b>Easy Build System</b> Colliding with collider: " + colliders[i].name);
                    }

                    if (buildingCollisionSurface != null && ContainsSurface(buildingCollisionSurface.Tag))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        bool ContainsSurface(string tag)
        {
            if (m_IgnoreBuildingSurface)
            {
                return true;
            }

            for (int i = 0; i < m_BuildingSurfaceTags.Length; i++)
            {
                if (tag == m_BuildingSurfaceTags[i])
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}