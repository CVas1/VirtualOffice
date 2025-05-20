using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour
{
    public static T Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)(object)this;
        }
        else
        {
            Destroy(this);
        }
    }
}