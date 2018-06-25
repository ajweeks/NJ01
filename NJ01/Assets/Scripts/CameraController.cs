using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController[] _players;

    private Vector3 _startingOffset;

    void Start ()
    {
        _players = new PlayerController[2];
        _players[0] = GameObject.Find("Player 0").GetComponent<PlayerController>();
        _players[1] = GameObject.Find("Player 1").GetComponent<PlayerController>();

        Vector3 centerPoint = GetCenterPoint();
        _startingOffset = (transform.position - centerPoint);
	}
	
	void Update ()
    {
        Vector3 centerPoint = GetCenterPoint();
        transform.position = new Vector3(centerPoint.x, 0, centerPoint.z) + _startingOffset;

	}

    Vector3 GetCenterPoint()
    {
        return ((_players[0].transform.position + _players[1].transform.position) / 2.0f);
    }
}
