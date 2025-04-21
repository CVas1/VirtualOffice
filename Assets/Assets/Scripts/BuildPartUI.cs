using System;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
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
        // Handle the button click event here
        Debug.Log("Part button clicked!");
        // You can add your logic to place the part in the game world or perform any other action.
    }
}
