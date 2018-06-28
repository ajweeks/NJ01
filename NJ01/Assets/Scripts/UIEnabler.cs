using UnityEngine;

public class UIEnabler : MonoBehaviour 
{
    // God damn canvas gettin in everyone's way gotta write a damn script to disable the damn thing until you need it
	void Start ()
	{
        gameObject.SetActive(true);
	}
}
