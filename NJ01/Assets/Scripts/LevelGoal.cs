using UnityEditor;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    int _playersContainedCount = 0;
    private GameObject[] _playersContained;

    private void Start()
    {
        _playersContained = new GameObject[2];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!ArrayUtility.Contains(_playersContained, other.gameObject))
            {
                _playersContained[_playersContainedCount] = other.gameObject;
                ++_playersContainedCount;

                if (_playersContainedCount == 2)
                {
                    LevelManager.EnterNextLevel();
                }
            }
        }
    }

}
