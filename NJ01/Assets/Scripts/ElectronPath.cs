using UnityEngine;

public class ElectronPath : MonoBehaviour
{
    public Color BaseColor;
    public float BrightestEmmision = 2.0f;

    private Renderer _renderer;

	void Start ()
    {
        _renderer = GetComponent<Renderer>();
	}
	
	public void SetActiveLevel(float active) // [0, 1]   not active -> active
    {
        float val = active * BrightestEmmision;
        _renderer.material.SetColor("_EmissionColor", new Color(BaseColor.r * val, BaseColor.g * val, BaseColor.b * val));
    }
}
