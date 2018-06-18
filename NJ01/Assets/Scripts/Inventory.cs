using UnityEngine;

public class Inventory : MonoBehaviour
{

    static public int Keys;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
