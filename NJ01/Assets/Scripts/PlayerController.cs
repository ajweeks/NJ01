using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    private string _indexStr;

    private Rigidbody _rb;
    private bool _grounded = false;

    Quaternion _pRot;

    void Start ()
    {
        _rb = GetComponent<Rigidbody>();
        _indexStr = Index.ToString();
    }

    void Update ()
    {
        if (_rb.IsSleeping())
        {
            _rb.WakeUp();
        }

        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, 1.05f))
        {
            _grounded = true;
            Debug.DrawLine(transform.position, new Vector3(100, 100, 100));
        }

        float MoveH = Input.GetAxis("Move H " + _indexStr);
        float MoveV = Input.GetAxis("Move V " + _indexStr);

        Vector3 moveVec = new Vector3(MoveH, 0, MoveV) * MoveSpeed;
        if (!_grounded)
        {
            moveVec *= 0.5f;
        }
        transform.Translate(moveVec, Space.World);

        if (moveVec.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveVec.normalized);
            _pRot = transform.rotation;
        }
        else
        {
            transform.rotation = _pRot;
        }
    }
}
