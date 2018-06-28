using UnityEngine;

/*
 * Wish-list
 * - Attempt to use physics to simulate launch path rather than hacking it
 *   - Allow players to collide mid-air when jumping off different pads
 * - Better collision detection (Player.OnCollisionEnter rather than raycasts?)
 * - Indicate landing zone
 * - Shoot players different distances (based on mass?)
 * - Allow players to set launch pad rotation using valve
 */

public class LaunchPad : MonoBehaviour
{
    public Transform TargetPos;
    public float LaunchDurationSeconds = 2.0f;

    // How long the launch pad is in motion
    private float _flipDurationSeconds = 0.9f;

    private float _heightScale = 3.0f;

    private bool _launched = false;
    private float _secondsSinceLaunch = -1.0f;

    private PlayerController _playerLaunching;
    private Quaternion _startingRot;

    private Vector3 _playerPosInitialOffset;
    private Vector3 _playerPosLerpedOffset;
    private Vector3 _dPos;

    private bool _playerObstructed = false;

	void Start ()
    {
        _startingRot = transform.rotation;
        _dPos = (TargetPos.position - transform.position);
    }

    void Update ()
    {
		if (_launched)
        {
            if (_secondsSinceLaunch >= LaunchDurationSeconds)
            {
                if (!_playerObstructed)
                {
                    _playerLaunching.transform.position = TargetPos.position + _playerPosLerpedOffset;
                }
                transform.rotation = _startingRot;
                _launched = false;
                _playerLaunching = null;
                _playerObstructed = false;
                _secondsSinceLaunch = -1.0f;
            }
            else
            {
                if (_secondsSinceLaunch < _flipDurationSeconds)
                {
                    float angle = ((0.5f - Mathf.Abs(_secondsSinceLaunch / _flipDurationSeconds - 0.5f)) * 2.0f) * 90.0f;
                    angle = Mathf.Clamp(angle, 0.0f, 90.0f);
                    angle = -angle;
                    transform.rotation = _startingRot * Quaternion.AngleAxis(angle, Vector3.forward);
                }
                else
                {
                    transform.rotation = _startingRot;
                }

                // If player hit something in air, let gravity handle the rest
                if (!_playerObstructed)
                {
                    float t = (_secondsSinceLaunch / LaunchDurationSeconds);
                    _playerPosLerpedOffset = Vector3.Lerp(_playerPosInitialOffset, Vector3.zero, t);
                    _playerPosLerpedOffset.y = _playerPosInitialOffset.y;

                    Vector3 newPlayerPos = transform.position + _playerPosLerpedOffset + _dPos * t;
                    newPlayerPos.y -= (t * t - t) * _heightScale; // Parabolic curve
                    _playerLaunching.transform.position = newPlayerPos;

                    if (t > 0.25f) // Don't check for obstructions at beginning
                    {
                        Vector3 rayDir = _dPos;
                        rayDir.y = 0;
                        rayDir.Normalize();

                        Vector3 heightOffset = new Vector3(0, 1.0f, 0);
                        float maxDist = 1.0f;

                        bool validHit = false;

                        Ray rayHead = new Ray(_playerLaunching.transform.position + heightOffset, rayDir);
                        Debug.DrawLine(rayHead.origin, rayHead.origin + rayHead.direction * maxDist, Color.yellow);
                        RaycastHit rayHeadHit;
                        bool hit = Physics.Raycast(rayHead, out rayHeadHit, maxDist);
                        if (hit)
                        {
                            validHit = !rayHeadHit.collider.CompareTag("Launchpad");
                        }
                        else
                        {
                            Ray rayFeet = new Ray(_playerLaunching.transform.position - heightOffset, rayDir);
                            Debug.DrawLine(rayFeet.origin, rayFeet.origin + rayFeet.direction * maxDist, Color.yellow);
                            RaycastHit rayFeetHit;
                            hit |= Physics.Raycast(rayFeet, out rayFeetHit, maxDist);
                            if (hit)
                            {
                                validHit |= !rayFeetHit.collider.CompareTag("Launchpad");
                            }
                        }

                        if (validHit)
                        {
                            _playerObstructed = true;
                        }
                    }
                }

                _secondsSinceLaunch += Time.deltaTime;
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (_launched)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _launched = true;
            _secondsSinceLaunch = 0.0f;
            _playerLaunching = other.GetComponent<PlayerController>();
            _playerPosInitialOffset = _playerLaunching.transform.position - transform.position;
        }
    }
}
