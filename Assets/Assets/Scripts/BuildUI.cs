using System;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using UnityEngine;
using UnityEngine.UI;
public class BuildUI : MonoBehaviour
{
    public Transform buildPartContainer;
    public GameObject buildPartPrefab;
    
    [SerializeField] private Button buildButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button rotateButton;


    private void Start()
    {
        foreach (BuildingPart buildingPart in BuildingManager.Instance.BuildingPartReferences)
        {
            //create build part prefab
            GameObject buildPartUI = Instantiate(buildPartPrefab, buildPartContainer);
            buildPartUI.GetComponent<BuildPartUI>().buildingPart = buildingPart;
            Texture2D texture = buildingPart.GetGeneralSettings.Thumbnail;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            buildPartUI.GetComponent<Image>().sprite = sprite;
            
        }
    }
}
