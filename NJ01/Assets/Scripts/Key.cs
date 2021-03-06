﻿using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySound("coin-pickup");
            InventoryManager.Instance.AddKeys(1);
            Destroy(gameObject);
        }
    }
}
