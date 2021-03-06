﻿using UnityEngine;

public class Breakable : MonoBehaviour 
{
    private string _tagThatBreaks = "projectile";
    private bool _broken = false;
    private GameObject _brokenPartsParent = null;

	void Start ()
	{
        _brokenPartsParent = transform.Find("Parts").gameObject;
    }
	
    private void OnTriggerEnter(Collider other)
    {
        if (_broken)
        {
            return;
        }

        if (other.CompareTag(_tagThatBreaks))
        {
            _broken = true;

            GetComponent<MeshRenderer>().enabled = false;

            foreach (var box in GetComponents<BoxCollider>())
            {
                box.isTrigger = true;
            }

            _brokenPartsParent.SetActive(true);

            AudioManager.Instance.PlaySoundRandomized("breaking-glass");
        }
    }
}
