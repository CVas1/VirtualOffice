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
    [NonSerialized]public BuildingPartSelectionUI buildingPartSelectionUI;

    private void Start()
    {
        partButton = GetComponent<Button>();
        partButton.onClick.AddListener(OnPartButtonClicked);
    }

    private void OnPartButtonClicked()
    {
        buildingPartSelectionUI.Placing(buildingPart);
    }
}