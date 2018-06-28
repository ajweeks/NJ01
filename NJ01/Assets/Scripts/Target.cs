using UnityEngine;

public class Target : MonoBehaviour 
{
    public MovingBlock OutputBlock;

    public ElectronPath[] OutputPaths1;
    public ElectronPath[] OutputPaths2;

    public float SecondsToBeLit = 1.0f;

    private float _secondsLeftLit1 = 0;
    private float _secondsLeftLit2 = 0;

	void Update ()
	{
		if (_secondsLeftLit1 > 0)
        {
            _secondsLeftLit1 -= Time.deltaTime;
            _secondsLeftLit1 = Mathf.Max(_secondsLeftLit1, 0);

            float pathActiveState = Mathf.Clamp01(_secondsLeftLit1 / SecondsToBeLit);
            foreach (ElectronPath path in OutputPaths1)
            {
                path.SetActiveLevel(pathActiveState);
            }
        }

        if (_secondsLeftLit2 > 0)
        {
            _secondsLeftLit2 -= Time.deltaTime;
            _secondsLeftLit2 = Mathf.Max(_secondsLeftLit2, 0);

            float pathActiveState = Mathf.Clamp01(_secondsLeftLit2 / SecondsToBeLit);
            foreach (ElectronPath path in OutputPaths2)
            {
                path.SetActiveLevel(pathActiveState);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("projectile"))
        {
            other.GetComponent<Projectile>().HitTrigger = true;
            int newDir = OutputBlock.ToggleTargetPos();

            if (newDir == 0)
            {
                _secondsLeftLit1 = SecondsToBeLit;
            }
            else
            {
                _secondsLeftLit2 = SecondsToBeLit;
            }
        }
    }
}
