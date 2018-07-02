using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour 
{
    public float AirFlowSpeed = 10.0f;

    public Valve ControllingValve;

    private CapsuleCollider _airFlowCapsule;

    struct ProjectileRef
    {
        public GameObject obj;
        public Vector3 vel;
        public float y;
    }

    private List<ProjectileRef> _projectilesInAirStream;
    private Vector3 _airFlowDir;

    private float _minStickSpeed = 0.1f;
    private float _maxStickSpeed = 1.0f;
    private float _fallMultiplier = -60.0f;

    void Start ()
	{
        _projectilesInAirStream = new List<ProjectileRef>();

        _airFlowCapsule = GetComponentInChildren<CapsuleCollider>();
        int capsuleDir = _airFlowCapsule.direction;
        if (capsuleDir == 0)
        {
            _airFlowDir = Vector3.right;
        }
        else if (capsuleDir == 1)
        {
            _airFlowDir = Vector3.up;
        }
        else if (capsuleDir == 2)
        {
            _airFlowDir = Vector3.forward;
        }
    }

    void Update()
    {
        float stickSpeed = Mathf.Abs(ControllingValve.GetAverageStickSpeed());
        float stickSpeedMult = Mathf.Clamp01(Mathf.Max(stickSpeed - _minStickSpeed, 0.0f) / (_maxStickSpeed - _minStickSpeed));
        
            _projectilesInAirStream.ForEach(projectile =>
            {
                projectile.vel = new Vector3(
                    _airFlowDir.x * AirFlowSpeed * stickSpeedMult,
                    0,
                    _airFlowDir.z * AirFlowSpeed * stickSpeedMult);

                if (stickSpeedMult < 0.1f)
                {
                    projectile.y += _fallMultiplier * Time.deltaTime;
                }
            
                projectile.obj.transform.position = new Vector3(
                    projectile.obj.transform.position.x + projectile.vel.x * Time.deltaTime,
                    projectile.y,
                    projectile.obj.transform.position.z + projectile.vel.z * Time.deltaTime);
        });

        for (int i = _projectilesInAirStream.Count - 1; i >= 0; i--)
        {
            if (_projectilesInAirStream[i].obj.GetComponent<Projectile>().HitTrigger)
            {
                _projectilesInAirStream[i].obj.GetComponent<Rigidbody>().useGravity = true;
                _projectilesInAirStream.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("projectile"))
        {
            Projectile proj = other.GetComponent<Projectile>();

            if (!proj.HitTrigger)
            {
                foreach (var projectile in _projectilesInAirStream)
                {
                    if (projectile.obj == other.gameObject)
                    {
                        // This project has already been added
                        return;
                    }
                }

                ProjectileRef newRef;
                newRef.obj = other.gameObject;
                newRef.obj.GetComponent<Rigidbody>().useGravity = false;
                newRef.y = other.gameObject.transform.position.y;
                newRef.vel = other.GetComponent<Rigidbody>().velocity;
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _projectilesInAirStream.Add(newRef);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("projectile"))
        {
            foreach (var projectile in _projectilesInAirStream)
            {
                if (projectile.obj == other.gameObject)
                {
                    projectile.obj.GetComponent<Rigidbody>().useGravity = true;
                    _projectilesInAirStream.Remove(projectile);
                    return;
                }
            }
        }
    }
}
