using UnityEngine;

/*
 * Wish-list
 * - More intricate path (spline?)
 * - Able to be locked into place?
 * - One-way ratchet system (with release elsewhere?)
 * - RotatingBlock?
 */ 

public class MovingBlock : MonoBehaviour 
{
    private PlayerController _playerRiding;

    private void OnTriggerEnter(Collider other)
    {
        if (_playerRiding)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _playerRiding = other.GetComponent<PlayerController>();
            _playerRiding.SetBlockRiding(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_playerRiding)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _playerRiding.SetBlockRiding(null);
            _playerRiding = null;
        }
    }
}
