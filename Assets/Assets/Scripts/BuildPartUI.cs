using System;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using UnityEngine;
using UnityEngine.UI;

public class BuildPartUI : MonoBehaviour
{
    [SerializeField] private Button partButton;
    [SerializeField] private Image partImage;
    public BuildingPart buildingPart;

    private void Start()
    {
        partButton = GetComponent<Button>();
        partButton.onClick.AddListener(OnPartButtonClicked);
    }

    private void OnPartButtonClicked()
    {
        BuildingPlacer.Instance.SelectBuildingPart(buildingPart);
        BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
        //cursor locked
        Cursor.lockState = CursorLockMode.Locked;
    }
}