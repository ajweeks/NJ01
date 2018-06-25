using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    public GameObject TrajectoryPlanePrefab;
    public GameObject ProjectilePrefab;

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

    private GameObject _trajectoryPlane = null;
    private MeshRenderer _trajectoryPlaneMesh = null;
    private float _trajectoryPlaneMatOffset = 0;
    private float _trajectoryPlaneOffsetSpeed = 3.0f;

    private RollingAverage _averageInteractStickLength;
    private RollingAverage _averageInteractDirection;

    void Start ()
    {
        _rb = GetComponent<Rigidbody>();
        _indexStr = Index.ToString();

        _trajectoryPlane = Instantiate(TrajectoryPlanePrefab);
        _trajectoryPlaneMesh = _trajectoryPlane.GetComponentInChildren<MeshRenderer>();
        _trajectoryPlane.SetActive(false);

        _averageInteractStickLength = new RollingAverage();
        _averageInteractStickLength.Create(5);
        _averageInteractDirection = new RollingAverage();
        _averageInteractDirection.Create(6);
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
            _averageInteractStickLength.Clear();
        }
        else
        {
            float horizontal = Input.GetAxis("Interact H " + _indexStr);
            float vertical = Input.GetAxis("Interact V " + _indexStr);

            Helpers.CleanupAxes(ref horizontal, ref vertical);

            float minimumExtensionLength = 0.1f;
            Vector2 stickExtension = new Vector2(horizontal, vertical);
            float extensionLength = stickExtension.magnitude;

            _averageInteractStickLength.AddValue(extensionLength);

            if (extensionLength > minimumExtensionLength)
            {
                float currentAverageStickDir = _averageInteractDirection.CurrentAverage;
                float currentStickDir = Mathf.Atan2(vertical, horizontal) + Mathf.PI;
                if (_pAiming)
                {
                    // When stick is closer to center weight less heavily
                    currentStickDir = Mathf.Lerp(currentAverageStickDir, currentStickDir, extensionLength);
                    _averageInteractDirection.AddValue(currentStickDir);
                }
                else
                {
                    _trajectoryPlane.SetActive(true);
                    // This prevents the plane from always starting with a rotation of 0 rad
                    _averageInteractDirection.SetAllValues(currentStickDir);
                }


                Debug.Log(_averageInteractDirection.CurrentAverage);

                _trajectoryPlaneMatOffset += _trajectoryPlaneOffsetSpeed * Time.deltaTime;
                _trajectoryPlaneMesh.material.SetTextureOffset("_MainTex", new Vector2(0, -_trajectoryPlaneMatOffset));

                _trajectoryPlane.transform.position = transform.position;
                _trajectoryPlane.transform.rotation = Quaternion.LookRotation(
                    Quaternion.AngleAxis(Mathf.Rad2Deg * (3.0f * Mathf.PI / 2.0f - _averageInteractDirection.CurrentAverage - Mathf.PI), Vector3.up)
                    * Vector3.forward, Vector3.up);

                _pAiming = true;
            }
            else
            {
                _trajectoryPlane.SetActive(false);
                _pAiming = false;

                // If the average is still high then the player quickly released the stick
                // Treat as fire command
                if (_averageInteractStickLength.CurrentAverage > 0.4f)
                {
                    float forceMagnitude = 2000;

                    Vector3 forceDir = Quaternion.AngleAxis(Mathf.Rad2Deg * (3.0f * Mathf.PI / 2.0f - _averageInteractDirection.CurrentAverage - Mathf.PI), Vector3.up) * Vector3.forward;
                    forceDir.y += 0.1f;
                    forceDir.Normalize();
                    Vector3 force = forceDir * forceMagnitude * _averageInteractStickLength.CurrentAverage;

                    Vector3 projectilePos = transform.position + forceDir * 0.6f;
                    GameObject projectileInstance = Instantiate(ProjectilePrefab, projectilePos, Quaternion.identity);
                    projectileInstance.GetComponent<Rigidbody>().AddForce(force);

                    _averageInteractDirection.Clear();
                    _averageInteractStickLength.Clear();
                }
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
