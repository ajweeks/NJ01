using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController PC0;
    public PlayerController PC1;

    private Vector3 _startingOffset;

    void Start ()
    {
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
        return ((PC0.transform.position + PC1.transform.position) / 2.0f);
    }
}
