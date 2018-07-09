using UnityEngine;

/*
 * Wish-list
 * - Fade projectile trajectory plane in and out
 * - Snap trajectory dest height to geometry
 * - Projectile aim-assist (preview item to-be-hit)
 * - Turn player to face slingshot direction?
 * - Trajectory interacting with geometry (predict wall collision)
 * - Store Interact H & V averages separately to avoid jump between 6.28rad & 0 rad
 * - Sound effect on pull & release
 */

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    public GameObject TrajectoryPlanePrefab;
    public GameObject ProjectilePrefab;

    [HideInInspector]
    public Valve ValveInteractingWith = null;

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

    private float _maxProjectilePlaneTilingV = 9.5f;
    private float _projectileForceMagnitude = 2100;
    private float _projectileHeightAddition = 0.2f;

    private float _minY = -8.0f;

    void Start ()
    {
        _rb = GetComponent<Rigidbody>();
        _indexStr = Index.ToString();

        _trajectoryPlane = Instantiate(TrajectoryPlanePrefab);
        _trajectoryPlaneMesh = _trajectoryPlane.GetComponentInChildren<MeshRenderer>();
        _trajectoryPlane.SetActive(false);

        _averageInteractStickLength = new RollingAverage();
        _averageInteractStickLength.Create(10);
        _averageInteractDirection = new RollingAverage();
        _averageInteractDirection.Create(6);
    }

    void Update()
    {
        if (_rb.IsSleeping())
        {
            _rb.WakeUp();
        }

        if (transform.position.y <= _minY)
        {
            ResetLocation();
            return;
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

        if (ValveInteractingWith &&
            Mathf.Abs(ValveInteractingWith.GetMostRecentStickRotationSpeed()) > 0.1f)
        {
            MoveH = 0;
            MoveV = 0;
        }

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

        if (ValveInteractingWith)
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

            float minimumExtensionLength = 0.2f;
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

                _trajectoryPlaneMatOffset += _trajectoryPlaneOffsetSpeed * Time.deltaTime;
                _trajectoryPlaneMesh.material.SetTextureOffset("_MainTex", new Vector2(0, -_trajectoryPlaneMatOffset));
                float textureScaleV = Mathf.Lerp(0.1f, _maxProjectilePlaneTilingV, _averageInteractStickLength.CurrentAverage);
                _trajectoryPlaneMesh.material.SetTextureScale("_MainTex", new Vector2(1.0f, textureScaleV));

                _trajectoryPlane.transform.position = transform.position;
                _trajectoryPlane.transform.rotation = Quaternion.LookRotation(
                    Quaternion.AngleAxis(Mathf.Rad2Deg * (3.0f * Mathf.PI / 2.0f - _averageInteractDirection.CurrentAverage - Mathf.PI), Vector3.up)
                    * Vector3.forward, Vector3.up);

                Vector3 trajectoryPlaneScale = new Vector3(1.0f, 1.0f, _averageInteractStickLength.CurrentAverage);
                _trajectoryPlane.transform.localScale = trajectoryPlaneScale;

                _pAiming = true;
            }
            else
            {
                _trajectoryPlane.SetActive(false);
                _pAiming = false;

                // If the average is still high then the player quickly released the stick
                // Treat as fire command
                if (_averageInteractStickLength.CurrentAverage > 0.2f)
                {
                    Vector3 forceDir = Quaternion.AngleAxis(Mathf.Rad2Deg * (3.0f * Mathf.PI / 2.0f - _averageInteractDirection.CurrentAverage - Mathf.PI), Vector3.up) * Vector3.forward;
                    forceDir.y += _projectileHeightAddition;
                    forceDir.Normalize();
                    Vector3 force = forceDir * _projectileForceMagnitude * _averageInteractStickLength.CurrentAverage;

                    Vector3 projectilePos = transform.position + forceDir * 0.6f;
                    GameObject projectileInstance = Instantiate(ProjectilePrefab, projectilePos, Quaternion.identity);
                    projectileInstance.GetComponent<Rigidbody>().AddForce(force);

                    AudioManager.Instance.PlaySoundRandomized("whoosh");

                    _averageInteractDirection.Clear();
                    _averageInteractStickLength.Clear();
                }
            }
        }
    }

    private void ResetLocation()
    {
        LevelManager.Instance.ReloadLevel();
    }

    public void SetBlockRiding(MovingBlock block)
    {
        if (_blockRiding == block)
        {
            return;
        }

        if (_blockRiding != null)
        {
            _blockRiding.RemoveRider(this);
        }

        _blockRiding = block;

        if (_blockRiding)
        {
            _blockRidingPrevPos = _blockRiding.transform.position;
        }
    }
}
