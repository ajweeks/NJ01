using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    public GameObject TrajectoryPlanePrefab;

    [HideInInspector]
    public bool InteractingWithValve = false;

    private float _turnSpeed = 2000.0f;

    private string _indexStr;

    private Rigidbody _rb;
    private bool _grounded = false;

    private Quaternion _pRot;
    private MovingBlock _blockRiding = null;
    private Vector3 _blockRidingPrevPos;

    private float _aimingDirection = -1.0f;
    private bool _pAiming = false;

    GameObject _trajectoryPlane = null;

    void Start ()
    {
        _rb = GetComponent<Rigidbody>();
        _indexStr = Index.ToString();

        _trajectoryPlane = Instantiate(TrajectoryPlanePrefab);
        _trajectoryPlane.SetActive(false);
    }

    void Update()
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

        Vector3 translation = Vector3.zero;

        if (_blockRiding)
        {
            translation = (_blockRiding.transform.position - _blockRidingPrevPos);
            _blockRidingPrevPos = _blockRiding.transform.position;
        }

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

        transform.Translate(translation + moveVec, Space.World);


        if (moveVec.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(_pRot, Quaternion.LookRotation(moveVec.normalized), Time.deltaTime * _turnSpeed);
            _pRot = transform.rotation;
        }
        else
        {
            transform.rotation = _pRot;
        }

        if (InteractingWithValve)
        {
            _pAiming = false;
            _trajectoryPlane.SetActive(false);
        }
        else
        {
            float horizontal = Input.GetAxis("Interact H " + _indexStr);
            float vertical = Input.GetAxis("Interact V " + _indexStr);

            Helpers.CleanupAxes(ref horizontal, ref vertical);

            float minimumExtensionLength = 0.1f;
            float extensionLength = new Vector2(horizontal, vertical).magnitude;

            if (extensionLength > minimumExtensionLength)
            {
                if (!_pAiming)
                {
                    _trajectoryPlane.SetActive(true);
                }

                _trajectoryPlane.transform.position = transform.position;
                Vector3 trajectoryPlaneForward = new Vector3(-horizontal, 0, -vertical);
                trajectoryPlaneForward.Normalize();
                _trajectoryPlane.transform.rotation = Quaternion.LookRotation(trajectoryPlaneForward, Vector3.up);

                _pAiming = true;
            }
            else
            {
                _trajectoryPlane.SetActive(false);
                _pAiming = false;
            }
        }
    }

    public void SetBlockRiding(MovingBlock block)
    {
        _blockRiding = block;

        if (_blockRiding)
        {
            _blockRidingPrevPos = _blockRiding.transform.position;
        }
    }
}
