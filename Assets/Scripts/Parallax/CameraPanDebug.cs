using UnityEngine;

public class CameraPanDebug : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        transform.position += new Vector3(h, v, 0f) * (speed * Time.deltaTime);
    }
}
