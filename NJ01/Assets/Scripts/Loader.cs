using UnityEngine;

public class Loader : MonoBehaviour 
{
    public GameObject audioManager;

	void Awake()
	{
		if (AudioManager.instance == null)
        {
            Instantiate(audioManager);
        }
	}
}
