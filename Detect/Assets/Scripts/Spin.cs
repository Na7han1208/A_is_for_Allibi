using UnityEngine;

public class Spin : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.forward;
    public float speed = 10f;

    void Update()
    {
        transform.Rotate(rotationAxis * speed * Time.deltaTime, Space.Self);
    }
}