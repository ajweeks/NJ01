using UnityEngine;

public class Loader : MonoBehaviour 
{
    public GameObject audioManager;
    public GameObject levelManager;
    public GameObject inventoryManager;


    void Awake()
	{
		if (AudioManager.Instance == null)
        {
            Instantiate(audioManager);
        }

        if (LevelManager.Instance == null)
        {
            Instantiate(levelManager);
        }

        if (InventoryManager.Instance == null)
        {
            Instantiate(inventoryManager);
        }
	}
}
