using UnityEngine;

public class Door : MonoBehaviour
{
    public int RequiredKeyCount = 1;
    public float OpenRotationYDeg = 90;

    public float OpenSpeed = 150.0f;

    private BoxCollider _trigger;

    private bool _open = false;
    private bool _opening = false;

	void Start ()
    {
        _trigger = GetComponentInChildren<BoxCollider>();
    }

    private void Update()
    {
        if (_opening)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, 
                Quaternion.AngleAxis(OpenRotationYDeg, Vector3.up),
                Time.deltaTime * OpenSpeed);

            if (Mathf.Abs(transform.localRotation.y - OpenRotationYDeg) < 0.01f)
            {
                _opening = false;
                _open = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Inventory.Keys >= RequiredKeyCount)
        {
            Open();
        }
    }

    private void Open()
    {
        if (!_open && !_opening)
        {
            _opening = true;
        }
    }
}
