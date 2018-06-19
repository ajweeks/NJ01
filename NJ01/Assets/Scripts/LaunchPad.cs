using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    public Transform TargetPos;
    public float LaunchDurationSeconds = 2.0f;

    // How long the launch pad is in motion
    private float _flipDurationSeconds = 0.9f;

    private float _heightAddition = 2.0f;

    private bool _launched = false;
    private float _secondsSinceLaunch = -1.0f;

    private PlayerController _playerLaunching;
    private Quaternion _startingRot;

    private Vector3 _playerPosOffset;
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
                    _playerLaunching.transform.position = TargetPos.position + _playerPosOffset;
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
                    Vector3 newPlayerPos = transform.position + _playerPosOffset + _dPos * t;
                    newPlayerPos.y += ((0.5f - Mathf.Abs(t - 0.5f)) * 2.0f) * _heightAddition;
                    _playerLaunching.transform.position = newPlayerPos;

                    if (t > 0.1f) // Don't check for obstructions at beginning
                    if (t > 0.1f) // Don't check for obstructions at beginning
                    {
                        Ray ray = new Ray(_playerLaunching.transform.position, _dPos.normalized);
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2.0f, Color.yellow);
                        RaycastHit rayHit;
                        if (Physics.Raycast(ray, out rayHit, 2.0f))
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
            _playerPosOffset = _playerLaunching.transform.position - transform.position;
        }
    }
}
