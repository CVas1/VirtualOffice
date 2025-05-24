using System;
using System.Collections.Generic;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPartSelectionUI : MonoBehaviour
{
    public Button wallButton;
    public Button furnitureButton;
    public Button propsButton;
    public Button interactableButton;

    public GameObject buildPartPrefab;
    public Transform buildPartContainer;

    [Header("Building Parts")] 
    public List<BuildingPart> wallParts;
    public List<BuildingPart> furnitureParts;
    public List<BuildingPart> propsParts;
    public List<BuildingPart> interactableParts;

    private void Start()
    {
        wallButton.onClick.AddListener(() => SetContentButtons(wallParts));
        furnitureButton.onClick.AddListener(() => SetContentButtons(furnitureParts));
        propsButton.onClick.AddListener(() => SetContentButtons(propsParts));
        interactableButton.onClick.AddListener(() => SetContentButtons(interactableParts));

        
        SetContentButtons(wallParts);
    }
    

    private void SetContentButtons(List<BuildingPart> buildingParts)
    {
        ClearContainer();

        foreach (BuildingPart buildingPart in buildingParts)
        {
            CreateBuildPartUI(buildingPart);
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in buildPartContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateBuildPartUI(BuildingPart buildingPart)
    {
        GameObject buildPartUIObject = Instantiate(buildPartPrefab, buildPartContainer);
        BuildPartUI buildPartUI = buildPartUIObject.GetComponent<BuildPartUI>();
        buildPartUI.buildingPartSelectionUI = this;
        buildPartUI.buildingPart = buildingPart;
        
        Texture2D texture = buildingPart.GetGeneralSettings.Thumbnail;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        buildPartUIObject.GetComponent<Image>().sprite = sprite;
    }
    
    public void Placing(BuildingPart buildingPart)
    {
        if (buildingPart == null) return;

        BuildingPlacer.Instance.SelectBuildingPart(buildingPart);
        BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
}