using UnityEngine;

/*
 * Wish-list
 * - Play sound/rumble controller  on min/max hit based on velocity
 * - Play sound on spin
 * - Draw arrows going in direction of force (spinning valve moves block, falling block spins valve)
 */

public class Valve : MonoBehaviour
{
    public enum OutputType
    {
        MOVING_BLOCK,
        FAN
    }

    public OutputType outputType;

    private PlayerController[] _players;

    public Transform OutputTransform;
    private Quaternion _outputTransformStartRot;

    public ElectronPath[] Paths; // All path objects which this valve "routes its power along" towards the output

    // How quickly this valve falls back to 0 when not being interacted with
    public float FallSpeed = 0.0f;

    private Quaternion _startingRot;
    private float _rotationScale = 50.0f; // Tune this to make valve rotate at same rate as joy stick

    public bool Bounded = true;

    public float OutputSpeed = 10.0f;
    public float OutputRotationMult = 90.0f;

    private RollingAverage[] _playerStickAvgs;
    private Material[] _defaultPlayerMaterials;
    private string[] _indexStrings;
    private int _interactingPlayerID = -1;

    private float _t; // [0, 1] represents percent rotated

    private float _secondsSinceAction = 0.0f;
    private float _turnPathMatCoolDown = 1.6f; // Seconds to wait after valve turn before reverting path mats

    private static float MAX_JOYSTICK_ROTATION_SPEED = 15.0f;

    private float _pPathActiveState;

    private bool pInDeadzone = true;
    private float pH = 0;
    private float pV = 0;

    private int _lastInteractCW = -1;

    void Start ()
    {
        _players = new PlayerController[2];
        _players[0] = GameObject.Find("Player 0").GetComponent<PlayerController>();
        _players[1] = GameObject.Find("Player 1").GetComponent<PlayerController>();

        _indexStrings = new string[2] { "0", "1" };

        _defaultPlayerMaterials = new Material[2];
        _defaultPlayerMaterials[0] = _players[0].GetComponent<Renderer>().material;
        _defaultPlayerMaterials[1] = _players[1].GetComponent<Renderer>().material;

        RollingAverage p1a = new RollingAverage();
        p1a.Create(8);
        RollingAverage p2a = new RollingAverage();
        p2a.Create(8);
        _playerStickAvgs = new RollingAverage[2] { p1a, p2a };

        _outputTransformStartRot = OutputTransform.rotation;
        _startingRot = transform.rotation;

        // Start fully cooled-down
        _secondsSinceAction = _turnPathMatCoolDown;
    }

    void Update ()
    {
        bool bAction = false;
        bool bShouldRotate = false;

        if (_interactingPlayerID != -1)
        {
            float horizontal = Input.GetAxis("Interact H " + _indexStrings[_interactingPlayerID]);
            float vertical = Input.GetAxis("Interact V " + _indexStrings[_interactingPlayerID]);

            Helpers.CleanupAxes(ref horizontal, ref vertical);

            float minimumExtensionLength = 0.1f;
            float extensionLength = new Vector2(horizontal, vertical).magnitude;

            if (Input.GetButtonDown("Interact " + _indexStrings[_interactingPlayerID]))
            {
                _lastInteractCW = -_lastInteractCW;
            }

            if (Input.GetButton("Interact " + _indexStrings[_interactingPlayerID]))
            {
                float stickRotationSpeed = 10.0f * Time.deltaTime * _lastInteractCW;
                bAction = true;
                _playerStickAvgs[_interactingPlayerID].AddValue(stickRotationSpeed);
                bShouldRotate = true;
            }
            else if (extensionLength > minimumExtensionLength)
            {
                if (!pInDeadzone)
                {
                    float currentAngle = Mathf.Atan2(vertical, horizontal) + Mathf.PI;
                    float previousAngle = Mathf.Atan2(pV, pH) + Mathf.PI;
                    // Asymptote occurs on left
                    if (horizontal < 0.0f)
                    {
                        if (pV < 0.0f && vertical >= 0.0f)
                        {
                            // CCW
                            currentAngle -= Mathf.PI * 2.0f;
                        }
                        else if (pV >= 0.0f && vertical < 0.0f)
                        {
                            // CW
                            currentAngle += Mathf.PI * 2.0f;
                        }
                    }

                    float stickRotationSpeed = -(currentAngle - previousAngle);
                    // Don't take into account spin as much when joystick is close to centered
                    stickRotationSpeed *= extensionLength;
                    stickRotationSpeed = Mathf.Clamp(stickRotationSpeed,
                                                    -MAX_JOYSTICK_ROTATION_SPEED,
                                                    MAX_JOYSTICK_ROTATION_SPEED);

                    if (Mathf.Abs(stickRotationSpeed) > 0.001f)
                    {
                        bAction = true;
                        bShouldRotate = true;
                    }

                    _playerStickAvgs[_interactingPlayerID].AddValue(stickRotationSpeed);
                }

                pInDeadzone = false;
            }
            else
            {
                _playerStickAvgs[_interactingPlayerID].Clear();
                pInDeadzone = true;
            }

            if (bShouldRotate)
            {
                pH = horizontal;
                pV = vertical;
            }
        }

        if (bShouldRotate)
        {
            float rotSpeed = _playerStickAvgs[_interactingPlayerID].CurrentAverage;
            _t += rotSpeed * OutputSpeed * Time.deltaTime;
        }

        if (!bAction)
        {
            _t -= FallSpeed * Time.deltaTime;
        }

        if (Bounded)
        {
            _t = Mathf.Clamp01(_t);
        }


        transform.rotation = _startingRot * Quaternion.AngleAxis(Mathf.Rad2Deg * _t * _rotationScale / OutputSpeed, Vector3.up);

        switch (outputType)
        {
            case OutputType.MOVING_BLOCK:
                OutputTransform.GetComponent<MovingBlock>().UpdatePosition(_t);
                break;
            case OutputType.FAN:
                OutputTransform.rotation = _outputTransformStartRot * Quaternion.AngleAxis(_t * OutputRotationMult, Vector3.right);
                break;
            default:
                break;
        }

        if (bAction)
        {
            _secondsSinceAction = 0.0f;
        }
        else
        {
            _secondsSinceAction += Time.deltaTime;
            _secondsSinceAction = Mathf.Clamp(_secondsSinceAction, 0.0f, _turnPathMatCoolDown);
        }

        // Always light the paths a little bit if a player is within range
        float baselineInteractingBrightness = (_interactingPlayerID == -1 ? 0.0f : 0.1f);
        float newPathActiveState = 1.0f - Mathf.Clamp01(_secondsSinceAction / _turnPathMatCoolDown - baselineInteractingBrightness);
        float pathActiveState = Mathf.Lerp(_pPathActiveState, newPathActiveState, 0.1f);
        foreach (ElectronPath path in Paths)
        {
            path.SetActiveLevel(pathActiveState);
        }
        _pPathActiveState = pathActiveState;
    }

    public float GetMostRecentStickRotationSpeed()
    {
        if (_interactingPlayerID == -1)
        {
            return 0;
        }

        return _playerStickAvgs[_interactingPlayerID].LatestEntry;
    }

    public float GetAverageStickSpeed()
    {
        if (_interactingPlayerID == -1)
        {
            return 0;
        }

        return _playerStickAvgs[_interactingPlayerID].CurrentAverage;
    }

    public void BeginInteract(int playerID)
    {
        _interactingPlayerID = playerID;
        _players[_interactingPlayerID].ValveInteractingWith = this;
    }

    public void EndInteract()
    {
        _playerStickAvgs[_interactingPlayerID].Clear();
        _players[_interactingPlayerID].ValveInteractingWith = null;
        _players[_interactingPlayerID].GetComponent<Renderer>().material = _defaultPlayerMaterials[_interactingPlayerID];
        _interactingPlayerID = -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
        if (pc && _interactingPlayerID == -1)
        {
            BeginInteract(pc.Index);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
        if (pc && pc.Index == _interactingPlayerID)
        {
            EndInteract();
        }
    }
}
