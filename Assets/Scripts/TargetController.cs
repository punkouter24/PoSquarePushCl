using UnityEngine;

public class TargetController : MonoBehaviour
{
    private Rigidbody targetRb;
    private float xRange = 4f;
    private float zRange = 4f;

    private void Start()
    {
        targetRb = GetComponent<Rigidbody>();
    }

    public void ResetTarget()
    {
        // Reset target position
        float x = Random.Range(-xRange, xRange);
        float z = Random.Range(-zRange, zRange);
        transform.localPosition = new Vector3(x, 0.5f, z);

        // Reset target velocity
        targetRb.linearVelocity = Vector3.zero;
        targetRb.angularVelocity = Vector3.zero;

        Debug.Log($"Target reset to position: {transform.localPosition}");
    }
}