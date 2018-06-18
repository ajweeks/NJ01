using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    static public int Keys { get { return _keys; } }

    static private int _keys;

    public static void RemoveKey()
    {
        if (_keys > 0)
        {
            --_keys;
            UpdateText();
        }
    }

    public static void AddKey()
    {
        ++_keys;
        UpdateText();
    }

    private static void UpdateText()
    {
        Text keyCountText = GameObject.Find("KeyCount").GetComponent<Text>();
        if (_keys > 0)
        {
            keyCountText.text = _keys.ToString();
        }
        else
        {
            keyCountText.text = "";
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
