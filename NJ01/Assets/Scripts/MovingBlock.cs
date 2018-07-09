using UnityEngine;

/*
 * Wish-list
 * - More intricate path (spline?)
 * - Able to be locked into place?
 * - One-way ratchet system (with release elsewhere?)
 * - RotatingBlock?
 */ 

public class MovingBlock : MonoBehaviour 
{
    public bool DrawPath = true;
    public bool AutoReturn = false;

    public float SecondsToReachTargetPos = 2.0f;

    public Transform EndPos;

    private PlayerController[] _playersRiding;

    private Vector3 _startPos;
    private Vector3 _dPos;

    private bool _bMovingToEndPos = false;
    private bool _bMovingToStartPos = false;

    private float _secondsToArrival;

    private void Start()
    {
        _playersRiding = new PlayerController[2];

        _startPos = transform.position;
        if (EndPos)
        {
            _dPos = (EndPos.position - _startPos);
        }
    }

    private float CalculateSecondsToArrival()
    {
        return Mathf.Clamp01((transform.position - EndPos.position).magnitude / _dPos.magnitude) * SecondsToReachTargetPos;
    }

    /* Returns 0 if moving to end, 1 otherwise */
    public int ToggleTargetPos()
    {
        if (_bMovingToStartPos)
        {
            StartMovingToEndPos();
            return 0;
        }
        else if (_bMovingToEndPos)
        {
            StartMovingToStartPos();
            return 1;
        }
        else
        {
            if (CalculateSecondsToArrival() > SecondsToReachTargetPos / 2.0f)
            {
                StartMovingToEndPos();
                return 0;
            }
            else
            {
                StartMovingToStartPos();
                return 1;
            }
        }
    }

    public void StartMovingToEndPos()
    {
        _bMovingToEndPos = true;
        _bMovingToStartPos = false;

        _secondsToArrival = CalculateSecondsToArrival();
    }

    public void StartMovingToStartPos()
    {
        _bMovingToStartPos = true;
        _bMovingToEndPos = false;

        _secondsToArrival = SecondsToReachTargetPos - CalculateSecondsToArrival();
    }

    public void UpdatePosition(float t)
    {
        if (_bMovingToStartPos || _bMovingToEndPos)
        {
            Debug.LogError("Attempted to update moving block pos while moving to start/end pos!");
            return;
        }

        transform.position = _startPos + (t * _dPos);
    }

    private void Update()
    {
        if (DrawPath)
        {
            Debug.DrawLine(_startPos, EndPos.position, Color.red, -1, false);

            if (_playersRiding[0])
            {
                Debug.DrawLine(transform.position, _playersRiding[0].transform.position, Color.cyan);
            }

            if (_playersRiding[1])
            {
                Debug.DrawLine(transform.position, _playersRiding[1].transform.position, Color.cyan);
            }
        }

        if (_bMovingToStartPos)
        {
            _secondsToArrival -= Time.deltaTime;

            float percentToTargetPos = 1.0f - (_secondsToArrival / SecondsToReachTargetPos);

            if (_secondsToArrival <= 0.0f)
            {
                _secondsToArrival = 0.0f;
                percentToTargetPos = 1.0f;
                _bMovingToStartPos = false;
            }

            transform.position = _startPos + ((1.0f - percentToTargetPos) * _dPos);
        }
        else if(_bMovingToEndPos)
        {
            _secondsToArrival -= Time.deltaTime;

            float percentToTargetPos = 1.0f - (_secondsToArrival / SecondsToReachTargetPos);

            if (_secondsToArrival <= 0.0f)
            {
                _secondsToArrival = 0.0f;
                percentToTargetPos = 1.0f;
                _bMovingToEndPos = false;

                if (AutoReturn)
                {
                    StartMovingToStartPos();
                }
            }

            transform.position = _startPos + (percentToTargetPos * _dPos);
        }
    }

    public void RemoveRider(PlayerController pc)
    {
        if (_playersRiding[0] == pc)
        {
            _playersRiding[0] = null;

            if (_playersRiding[1])
            {
                _playersRiding[0] = _playersRiding[1];
                _playersRiding[1] = null;
            }
        }
        else if (_playersRiding[1] == pc)
        {
            _playersRiding[1] = null;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_playersRiding[0] && _playersRiding[1])
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerController pc = _playersRiding[0] ? _playersRiding[1] : _playersRiding[0];

            pc = other.GetComponent<PlayerController>();
            pc.SetBlockRiding(this);
            if (_playersRiding[0])
            {
                _playersRiding[1] = pc;
            }
            else
            {
                _playersRiding[0] = pc;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController otherPC = other.GetComponent<PlayerController>();
            if (otherPC == _playersRiding[0])
            {
                _playersRiding[0].SetBlockRiding(null);
                _playersRiding[0] = null;

                if (_playersRiding[1])
                {
                    _playersRiding[0] = _playersRiding[1];
                    _playersRiding[1] = null;
                }
            }
            else if (otherPC == _playersRiding[1])
            {
                _playersRiding[1].SetBlockRiding(null);
                _playersRiding[1] = null;
            }
        }
    }
}
