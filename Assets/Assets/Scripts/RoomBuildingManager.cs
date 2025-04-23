using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace Assets.Scripts
{
    public class RoomBuildingManager : MonoBehaviour
    {
        private bool isLoading = false;

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
            Save();
        }

        private void OnPlacingBuildingPart(BuildingPart buildingPart)
        {
            Debug.Log("OnPlacingBuildingPart");
            if (isLoading) return;
            if (buildingPart == null) return;
            if (buildingPart.State != BuildingPart.StateType.PLACED) return;
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
                GameManager.Instance.SaveBuildingData(saveDataJson);
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

            if (BuildingManager.Instance.RegisteredBuildingParts.Count > 0)
            {
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
            }


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