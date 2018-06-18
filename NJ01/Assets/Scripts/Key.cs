using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory.Keys++;
            Destroy(gameObject);

            Text keyCountText = GameObject.Find("KeyCount").GetComponent<Text>();
            if (keyCountText.text.Length == 0)
            {
                keyCountText.text = "1";
            }
            else
            {
                keyCountText.text = (int.Parse(keyCountText.text) + 1).ToString();
            }
        }
    }
}
