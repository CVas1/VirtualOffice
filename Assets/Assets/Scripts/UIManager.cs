using System;
using System.Linq;
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

        [Header("Stats UI")] [SerializeField] private GameObject ShowStatsButton;
        [SerializeField] private GameObject StatsPanel;
        [SerializeField] private Button Stats24hButton;
        [SerializeField] private Button Stats7dButton;
        [SerializeField] private Button Stats1mButton;
        [SerializeField] private Transform StatsContentParent;
        [SerializeField] private GameObject StatsUserEntryPrefab;

        [Header("Mouse Visibility")] private float lastClickTime = 0f;
        [SerializeField] private float catchTime = 0.25f;

        private enum StatsRange
        {
            Last24h,
            Last7d,
            Last1m
        }

        private StatsRange currentStatsRange = StatsRange.Last24h;

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
            QuitButton.SetActive(false);
            buildMenu.SetActive(false);
            SetCursorVisibilty(false);

            // Show or hide the ShowStats button based on stats count
            if (STDBBackendManager.Instance.roomStatsManager.Stats.Count > 0)
            {
                ShowStatsButton.SetActive(true);

                StatsPanel.SetActive(false);
                Stats24hButton.onClick.AddListener(() => ShowStats(StatsRange.Last24h));
                Stats7dButton.onClick.AddListener(() => ShowStats(StatsRange.Last7d));
                Stats1mButton.onClick.AddListener(() => ShowStats(StatsRange.Last1m));
                // ShowStatsButton.GetComponent<Button>().onClick.AddListener(OpenStatsPanel);
            }
            else
                ShowStatsButton.SetActive(false);
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left-click
            {
                float timeSinceLastClick = Time.time - lastClickTime;

                if (timeSinceLastClick <= catchTime)
                {
                    // Double-click detected: toggle cursor visibility
                    Cursor.visible = !Cursor.visible;
                    Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
                }

                lastClickTime = Time.time;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
            {
                MailBoxOpenClose();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //setactive quitroom and stats buttons
                if (ShadowPanel.activeSelf)
                {
                    ShadowPanel.SetActive(false);
                    QuitButton.SetActive(false);
                    ShowStatsButton.SetActive(false);
                    SetCursorVisibilty(false);
                }
                else
                {
                    ShadowPanel.SetActive(true);
                    QuitButton.SetActive(true);
                    ShowStatsButton.SetActive(STDBBackendManager.Instance.roomStatsManager.Stats.Count > 0);
                    SetCursorVisibilty(true);
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
            {
                OpenStatsPanel();
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

        public void OpenStatsPanel()
        {
            if (STDBBackendManager.Instance.roomStatsManager.Stats.Count == 0) return;

            if (StatsPanel.activeSelf)
            {
                StatsPanel.SetActive(false);
                SetCursorVisibilty(false);
            }
            else
            {
                StatsPanel.SetActive(true);
                SetCursorVisibilty(true);
                ShowStats(currentStatsRange); // Show last 24h stats by default
            }
        }

        private void ShowStats(StatsRange range)
        {
            currentStatsRange = range;
            // Clear previous entries
            foreach (Transform child in StatsContentParent)
                Destroy(child.gameObject);
            // Get stats for selected range
            var statsList = range switch
            {
                StatsRange.Last24h => STDBBackendManager.Instance.roomStatsManager.GetStatsLast24Hours(),
                StatsRange.Last7d => STDBBackendManager.Instance.roomStatsManager.GetStatsLast7Days(),
                StatsRange.Last1m => STDBBackendManager.Instance.roomStatsManager.GetStatsLastMonth(),
                _ => STDBBackendManager.Instance.roomStatsManager.GetStatsLast24Hours()
            };
            // Aggregate and sort by total duration descending
            var userStats = statsList
                .GroupBy(s => new { s.UserId, s.UserName })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    TotalDuration = g.Sum(s => (long)s.DurationMicroseconds)
                })
                .OrderByDescending(u => u.TotalDuration)
                .ToList();

            // Display each user's stats with rank
            int rank = 1;
            foreach (var user in userStats)
            {
                var entry = Instantiate(StatsUserEntryPrefab, StatsContentParent);
                var statsEntity = entry.GetComponent<StatsListEntity>();
                if (statsEntity != null)
                {
                    // Calculate TimeSpan from microseconds
                    TimeSpan ts = TimeSpan.FromMilliseconds(user.TotalDuration / 1000.0);

                    // If less than 1 minute, set to 1 minute
                    if (ts.TotalMinutes < 1)
                        ts = TimeSpan.FromMinutes(1);

                    // Build the value string
                    string value = "";
                    if (ts.Days > 0)
                        value += $"{ts.Days}d ";
                    if (ts.Hours > 0)
                        value += $"{ts.Hours}h ";
                    value += $"{ts.Minutes}m";

                    statsEntity.SetData(rank, user.UserName, value.Trim());
                }

                rank++;
            }
        }
    }
}