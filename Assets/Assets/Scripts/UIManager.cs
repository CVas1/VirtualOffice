using System;
using Assets.Scripts.Networking;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using SpacetimeDB.Types;
using TankAndHealerStudioAssets;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject ShadowPanel;
        [SerializeField] private GameObject QuitButton;
        [SerializeField] private UltimateChatBox ChatPanel;
        [SerializeField] private GameObject MailPanel;

        [Header("Build Menu")] [SerializeField]
        private GameObject buildMenu;

        [SerializeField] private BuildingPartSelectionUI buildingPartSelectionUI;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(Instance);
                Instance = this;
            }
        }

        private void Start()
        {
            buildMenu.SetActive(false);
            SetCursorVisibilty(false);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                MailBoxOpenClose();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);

                if (!buildMenu.gameObject.activeSelf)
                {
                    buildingPartSelectionUI.gameObject.SetActive(true);
                    SetCursorVisibilty(true);
                    buildMenu.SetActive(true);
                }
                else if (buildingPartSelectionUI.gameObject.activeSelf)
                {
                    SetCursorVisibilty(false);
                    buildMenu.SetActive(false);
                }
                else
                {
                    buildingPartSelectionUI.gameObject.SetActive(true);
                    SetCursorVisibilty(true);
                }
            }
        }

        private void MailBoxOpenClose()
        {
            if (UIMailPanel.Instance.mailBoxPanel.gameObject.activeSelf ||
                UIMailPanel.Instance.mailReadPanel.gameObject.activeSelf ||
                UIMailPanel.Instance.mailWritePanel.gameObject.activeSelf)
            {
                UIMailPanel.Instance.CloseMailBoxPanel();
                SetCursorVisibilty(false);
            }
            else
            {
                UIMailPanel.Instance.OpenMailBoxPanel();
                SetCursorVisibilty(true);
            }
        }

        private void SetCursorVisibilty(bool setCursorVisible)
        {
            if (Cursor.lockState == CursorLockMode.Locked && setCursorVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Cursor.lockState == CursorLockMode.None && !setCursorVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void QuitRoom()
        {
            STDBBackendManager.Instance.roomManager.LeaveRoom();
            SetCursorVisibilty(true);
        }
    }
}