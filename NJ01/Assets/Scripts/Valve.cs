using UnityEngine;

public class Valve : MonoBehaviour
{
    public PlayerController[] Players;
    public Material InteractMat;

    public Transform EndPos;
    public Transform OutputTransform;
    private Vector3 _outputTransformStartPos;

    public ElectronPath[] Paths; // All path objects which this valve "routes its power along" towards the output

    public bool DrawPath = true;

    private Quaternion _startingRot;
    private float _rotationScale = 50.0f; // Tune this to make valve rotate at same rate as joy stick

    public float OutputSpeed = 10.0f;

    private RollingAverage[] _playerStickAvgs;
    private Material[] _defaultPlayerMaterials;
    private string[] _indexStrings;
    private int _interactingPlayerID = -1;

    private float _t; // [0, 1] represents percent rotated

    //private float MaxRotation = Mathf.PI;
    //private float _currentRotation; // [0, MaxRotation]
    
    private float _secondsSinceAction = 0.0f;
    private float _turnPathMatCoolDown = 1.6f; // Seconds to wait after valve turn before reverting path mats

    private static float MAX_JOYSTICK_ROTATION_SPEED = 15.0f;

    private float _pPathActiveState;

    private bool pInDeadzone = true;
    private float pH = 0;
    private float pV = 0;

    private int _lastInteractCW = 1;
    void Start ()
    {
        _indexStrings = new string[2] { "0", "1" };

        _defaultPlayerMaterials = new Material[2];
        _defaultPlayerMaterials[0] = Players[0].GetComponent<Renderer>().material;
        _defaultPlayerMaterials[1] = Players[1].GetComponent<Renderer>().material;

        RollingAverage p1a = new RollingAverage();
        p1a.Create(8);
        RollingAverage p2a = new RollingAverage();
        p2a.Create(8);
        _playerStickAvgs = new RollingAverage[2] { p1a, p2a };

        _outputTransformStartPos = OutputTransform.position;
        _startingRot = transform.rotation;

        // Start fully cooled-down
        _secondsSinceAction = _turnPathMatCoolDown;
    }

    void Update ()
    {
        bool bAction = false;

        if (_interactingPlayerID != -1)
        {
            float horizontal = Input.GetAxis("Interact H " + _indexStrings[_interactingPlayerID]);
            float vertical = Input.GetAxis("Interact V " + _indexStrings[_interactingPlayerID]);
            float minimumExtensionLength = 0.35f;
            float extensionLength = new Vector2(horizontal, vertical).magnitude;

            bool bShouldRotate = false;

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

                    float stickRotationSpeed = (currentAngle - previousAngle);
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

                float rotSpeed = _playerStickAvgs[_interactingPlayerID].CurrentAverage;
                _t += rotSpeed * OutputSpeed * Time.deltaTime;
                _t = Mathf.Clamp01(_t);


                Vector3 dPos = (EndPos.position - _outputTransformStartPos);

                if (DrawPath)
                {
                    Debug.DrawLine(_outputTransformStartPos, EndPos.position, Color.red, -1, false);
                }

                transform.rotation = _startingRot * Quaternion.AngleAxis(-Mathf.Rad2Deg * _t * _rotationScale / OutputSpeed, Vector3.up);

                OutputTransform.position = _outputTransformStartPos + (_t * dPos);

            }

            if (bAction)
            {
                _secondsSinceAction = 0.0f;
            }
            else
            {
                _secondsSinceAction += Time.deltaTime;
            }

            float newPathActiveState = 1.0f - Mathf.Clamp01(_secondsSinceAction / _turnPathMatCoolDown);
            float pathActiveState = Mathf.Lerp(_pPathActiveState, newPathActiveState, 0.1f);
            foreach (ElectronPath path in Paths)
            {
                path.SetActiveLevel(pathActiveState);
            }
            _pPathActiveState = pathActiveState;
        }
    }

    public void BeginInteract(int playerID)
    {
        _interactingPlayerID = playerID;
        Players[_interactingPlayerID].GetComponent<Renderer>().material = InteractMat;
    }

    public void EndInteract()
    {
        _playerStickAvgs[_interactingPlayerID].Clear();
        Players[_interactingPlayerID].GetComponent<Renderer>().material = _defaultPlayerMaterials[_interactingPlayerID];
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

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
