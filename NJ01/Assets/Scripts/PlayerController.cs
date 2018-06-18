using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Index;
    public float MoveSpeed = 10.0f;

    private string _indexStr;

    private Rigidbody _rb;
    private bool _grounded = false;

    void Start ()
    {
        _rb = GetComponent<Rigidbody>();
        _indexStr = Index.ToString();
    }

    void Update ()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, 1.1f))
        {
            _grounded = true;
        }

        float MoveH = Input.GetAxis("Move H " + _indexStr);
        float MoveV = Input.GetAxis("Move V " + _indexStr);

        transform.Translate(new Vector3(MoveH, 0, MoveV) * MoveSpeed);

        //transform.rotation = Quaternion.identity;
    }
}
