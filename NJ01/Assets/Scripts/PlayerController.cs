using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    private float _turnSpeed = 2000.0f;

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
        }

        float MoveH = Input.GetAxis("Move H " + _indexStr);
        float MoveV = Input.GetAxis("Move V " + _indexStr);

        Helpers.CleanupAxes(ref MoveH, ref MoveV);

        Vector3 moveVec = new Vector3(MoveH, 0, MoveV);
        if (moveVec.sqrMagnitude > 1.0f)
        {
            moveVec.Normalize();
        }
        moveVec *= MoveSpeed;

        if (!_grounded)
        {
            moveVec *= 0.5f;
        }
        transform.Translate(moveVec, Space.World);

        if (moveVec.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(_pRot, Quaternion.LookRotation(moveVec.normalized), Time.deltaTime * _turnSpeed);
            _pRot = transform.rotation;
        }
        else
        {
            transform.rotation = _pRot;
        }
    }
}
