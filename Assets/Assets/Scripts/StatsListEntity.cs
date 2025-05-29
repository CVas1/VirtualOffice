using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class StatsListEntity : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text valueText;

        public void SetData(int rank, string username, string value)
        {
            rankText.text = rank.ToString();
            usernameText.text = username;
            valueText.text = value;
        }
    }
}