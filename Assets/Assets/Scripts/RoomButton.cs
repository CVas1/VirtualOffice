using SpacetimeDB.Types;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class RoomButton : MonoBehaviour
    {
        private GameRoom room;
        [SerializeField] private TMP_Text roomNameText;
        public void Init(GameRoom roomData)
        {
            roomNameText.text = roomData.Name;
            room = roomData;
        }
        
        public void OnClickRoomButton()
        {
            if (room == null)
            {
                Debug.LogError("Room data is not initialized.");
                return;
            }
            
            // Call the method to join the room
            UIManager.Instance.OnClickJoinRoomMenu(room);
        }
    }
}