using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance = null;

    public int Keys { get { return _keys; } }
    private int _keys;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Clear()
    {
        _keys = 0;
        UpdateText();
    }

    public void RemoveKeys(int count)
    {
        if (_keys >= count)
        {
            _keys -= count;
            UpdateText();
        }
    }

    public void AddKeys(int count)
    {
        _keys += count;
        UpdateText();
    }

    private void UpdateText()
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
}
