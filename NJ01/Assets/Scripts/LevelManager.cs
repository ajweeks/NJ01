﻿using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance = null;

    public string LevelDirectory = "Assets/Scenes/Puzzles/";
    private string[] _levelNames;
    private int _levelIndex = 0;

    private bool _swappedLevels = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            var dir = new DirectoryInfo(LevelDirectory);
            FileInfo[] files = dir.GetFiles("*.unity");
            _levelNames = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                _levelNames[i] = files[i].Name.Split('.')[0];
            }

            //AudioManager.Instance.PlaySong("The_Lightworker");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < _levelNames.Length; ++i)
        {
            if (SceneManager.GetActiveScene().name == _levelNames[i])
            {
                _levelIndex = i;
                break;
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Prevent registering level cycle button presses twice
        if (_swappedLevels)
        {
            _swappedLevels = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadLevel();
            _swappedLevels = true;
        }

        if (Input.GetButtonDown("DEBUG next level"))
        {
            EnterNextLevel();
        }
        else if (Input.GetButtonDown("DEBUG previous level"))
        {
            EnterPreviousLevel();
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadSceneAsync(_levelNames[_levelIndex], LoadSceneMode.Single);
        InventoryManager.Instance.Clear();
    }

    public void EnterNextLevel()
    {
        ++_levelIndex;
        if (_levelIndex >= _levelNames.Length)
        {
            _levelIndex = 0;
        }
        _swappedLevels = true;

        SceneManager.LoadSceneAsync(_levelNames[_levelIndex], LoadSceneMode.Single);
        InventoryManager.Instance.Clear();
    }

    public void EnterPreviousLevel()
    {
        --_levelIndex;
        if (_levelIndex < 0)
        {
            _levelIndex = _levelNames.Length - 1;
        }
        _swappedLevels = true;

        SceneManager.LoadSceneAsync(_levelNames[_levelIndex], LoadSceneMode.Single);
        InventoryManager.Instance.Clear();
    }
}
