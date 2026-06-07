using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;   // Si no la pones, buscará la principal
    [Range(0f, 1f)] public float factorX = 0.2f;    // 0 = fijo, 1 = se mueve igual que la cámara
    [Range(0f, 1f)] public float factorY = 0.2f;

    private Vector3 _lastCamPos;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (targetCamera) _lastCamPos = targetCamera.transform.position;
    }

    void LateUpdate()
    {
        if (!targetCamera) return;

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - _lastCamPos;

        // Movimiento proporcional al desplazamiento de cámara
        transform.position += new Vector3(delta.x * factorX, delta.y * factorY, 0f);

        _lastCamPos = camPos;
    }
}
