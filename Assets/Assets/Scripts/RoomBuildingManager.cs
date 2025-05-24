using System;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Networking;
using UnityEngine;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace Assets.Scripts
{
    public class RoomBuildingManager : MonoBehaviour
    {
        private bool isLoading = false;

        [SerializeField] private BuildingPart projector;

        [SerializeField]
        private Dictionary<string, OfficeProjector> officeProjectors = new Dictionary<string, OfficeProjector>();

        public static RoomBuildingManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnRoomJoin()
        {
            Debug.Log("OnRoomJoin");
            isLoading = false;
            BuildingManager.Instance.OnDestroyingBuildingPartEvent.AddListener(OnDestroyingBuildingPart);
            BuildingManager.Instance.OnPlacingBuildingPartEvent.AddListener(OnPlacingBuildingPart);
        }

        public void OnRoomLeave()
        {
            Debug.Log("OnRoomLeave");
            BuildingManager.Instance.OnDestroyingBuildingPartEvent.RemoveListener(OnDestroyingBuildingPart);
            BuildingManager.Instance.OnPlacingBuildingPartEvent.RemoveListener(OnPlacingBuildingPart);

            DeleteAll();
        }

        private void OnDestroyingBuildingPart(BuildingPart buildingPart)
        {
            Debug.Log("OnDestroyingBuildingPart");
            if (isLoading) return;
            if (buildingPart == null) return;
            if (buildingPart.State != BuildingPart.StateType.DESTROY) return;

            if (buildingPart.GetGeneralSettings.Identifier == projector.GetGeneralSettings.Identifier)
            {
                foreach (var item in buildingPart.Properties)
                {
                    if (item.StartsWith("ProjectorId"))
                    {
                        STDBBackendManager.Instance.imageManager.ProjectorDestroyedLocally(item.Substring(11));
                        break;
                    }
                }
            }

            Save();
        }

        private void OnPlacingBuildingPart(BuildingPart buildingPart)
        {
            Debug.Log("OnPlacingBuildingPart");
            if (isLoading) return;
            if (buildingPart == null) return;
            if (buildingPart.State != BuildingPart.StateType.PLACED) return;

            if (buildingPart.GetGeneralSettings.Identifier == projector.GetGeneralSettings.Identifier)
            {
                bool found = false;
                foreach (var item in buildingPart.Properties)
                {
                    if (item.StartsWith("ProjectorId"))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    string projectorId = Guid.NewGuid().ToString();
                    buildingPart.Properties.Add("ProjectorId" + projectorId);
                    OfficeProjector officeProjector = buildingPart.GetComponent<OfficeProjector>();
                    officeProjector.SetProjectorId(projectorId);
                    officeProjectors.Add(projectorId, officeProjector);
                    STDBBackendManager.Instance.imageManager.ProjectorCreatedLocally(projectorId);
                }
            }

            Save();
        }

        [Serializable]
        public class SaveData
        {
            public List<BuildingPart.SaveSettings> Data = new List<BuildingPart.SaveSettings>();
        }

        public void Save()
        {
            Debug.Log("Save");
            if (BuildingManager.Instance.RegisteredBuildingParts.Count > 0)
            {
                SaveData saveData = new SaveData
                {
                    Data = new List<BuildingPart.SaveSettings>()
                };

                BuildingPart[] registeredBuildingParts = BuildingManager.Instance.RegisteredBuildingParts.ToArray();

                for (int i = 0; i < registeredBuildingParts.Length; i++)
                {
                    if (registeredBuildingParts[i] != null)
                    {
                        if (registeredBuildingParts[i].State != BuildingPart.StateType.PREVIEW)
                        {
                            BuildingPart.SaveSettings saveSettings = registeredBuildingParts[i].GetSaveData();

                            if (saveSettings != null)
                            {
                                saveData.Data.Add(saveSettings);
                            }
                        }
                    }
                }

                string saveDataJson = JsonUtility.ToJson(saveData);
                STDBBackendManager.Instance.buildingManager.SaveBuildingData(saveDataJson);
                // File.AppendAllText(path, JsonUtility.ToJson(saveData));
            }
        }

        public void Load(string jsonData)
        {
            Debug.Log("Load");
            isLoading = true;

            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

            if (saveData == null)
            {
                Debug.LogError("Failed to load save data.");
                return;
            }

            DeleteAll();
            isLoading = true;

            officeProjectors.Clear();
            for (int i = 0; i < saveData.Data.Count; i++)
            {
                if (saveData.Data[i] != null)
                {
                    BuildingPart buildingPart =
                        BuildingManager.Instance.GetBuildingPartByIdentifier(saveData.Data[i].Identifier);

                    if (buildingPart != null)
                    {
                        BuildingPart instancedBuildingPart =
                            BuildingManager.Instance.PlaceBuildingPart(buildingPart, saveData.Data[i].Position,
                                saveData.Data[i].Rotation, saveData.Data[i].Scale);

                        instancedBuildingPart.Properties = saveData.Data[i].Properties;

                        if (instancedBuildingPart.GetGeneralSettings.Identifier ==
                            projector.GetGeneralSettings.Identifier)
                        {
                            foreach (var item in instancedBuildingPart.Properties)
                            {
                                if (item.StartsWith("ProjectorId"))
                                {
                                    string projectorId = item.Substring(11);
                                    OfficeProjector officeProjector =
                                        instancedBuildingPart.GetComponent<OfficeProjector>();
                                    officeProjector.SetProjectorId(projectorId);
                                    officeProjectors.Add(projectorId, officeProjector);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("<b>Easy Build System</b> The Building Part reference with the name: <b>" +
                                         saveData.Data[i].Name + "</b> does not exist in Building Manager.");
                    }
                }
            }

            isLoading = false;
        }

        public void SetImageOfProjector(string projectorId, byte[] imageData, int width, int height)
        {
            if (projectorId == null) return;
            if (officeProjectors.ContainsKey(projectorId))
            {
                officeProjectors[projectorId].SetImage(imageData);
            }
        }

        // public OfficeProjector GetOfficeProjector(string projectorId)
        // {
        //     if (officeProjectors.ContainsKey(projectorId))
        //     {
        //         return officeProjectors[projectorId];
        //     }
        //
        //     return null;
        // }

        public void DeleteAll()
        {
            isLoading = true;

            BuildingPart[] registeredBuildingParts = BuildingManager.Instance.RegisteredBuildingParts.ToArray();

            for (int i = 0; i < registeredBuildingParts.Length; i++)
            {
                if (registeredBuildingParts[i] != null)
                {
                    if (registeredBuildingParts[i].State != BuildingPart.StateType.PREVIEW)
                    {
                        BuildingManager.Instance.DestroyBuildingPart(registeredBuildingParts[i]);
                    }
                }
            }

            isLoading = false;
        }
    }
}