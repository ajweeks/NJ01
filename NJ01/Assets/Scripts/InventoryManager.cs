﻿using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance = null;

    static public int Keys { get { return _keys; } }
    static private int _keys;

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
}