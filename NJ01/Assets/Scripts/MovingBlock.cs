using UnityEngine;

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
