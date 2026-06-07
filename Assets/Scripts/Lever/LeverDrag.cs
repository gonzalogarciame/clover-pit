using UnityEngine;

public class LeverDrag : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera cam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private HingeJoint hinge;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 200f;
    [SerializeField] private LayerMask leverLayerMask = ~0;

    [Header("Angles (POSITIVE PULL)")]
    [SerializeField] private float restAngle = 0f;
    [SerializeField] private float pullAngle = 45f;

    [Header("Smoothing")]
    [Tooltip("Tiempo aproximado para llegar al objetivo. Más alto = más suave/lento.")]
    [SerializeField] private float smoothTime = 0.18f;

    [Tooltip("Si quieres que al volver sea más lento/suave, pon >1 (ej: 1.2).")]
    [SerializeField] private float returnSmoothMultiplier = 1.2f;

    [Tooltip("Límite de velocidad angular en deg/s durante el suavizado.")]
    [SerializeField] private float maxSpeedDeg = 999f;

    [Header("Debug")]
    [SerializeField] private bool logToggle = false;

    [Header("End Lock")]
    [SerializeField] private float lockEpsilonDeg = 0.3f;  // si está a < 0.3º, lo clavamos
    [SerializeField] private float lockVelEpsilon = 0.02f; // si vel es pequeña, lo clavamos


    private bool targetPulled;
    private float targetAngle;
    private float vel; 
    private Quaternion restWorldRotation;


    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!hinge) hinge = GetComponent<HingeJoint>();

        if (!rb || !hinge)
        {
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        hinge.useMotor = false;
        hinge.useSpring = false;
        hinge.useLimits = true;

        targetAngle = restAngle;
        rb.WakeUp();

        restWorldRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryToggle();
    }

    void FixedUpdate()
    {
        SmoothRotateToTarget();
    }

    private void TryToggle()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, leverLayerMask))
            return;

        bool same = hit.transform == transform;
        bool child = hit.transform.IsChildOf(transform);
        bool parent = transform.IsChildOf(hit.transform);

        if (!same && !child && !parent)
            return;

        targetPulled = !targetPulled;
        targetAngle = targetPulled ? pullAngle : restAngle;

        rb.WakeUp();

        if (logToggle)
            Debug.Log("Toggle -> " + (targetPulled ? "PULL" : "REST"));
    }

    private void SmoothRotateToTarget()
    {
        // Angulo actual del hinge (en grados)
        float current = hinge.angle;

        float st = targetPulled ? smoothTime : (smoothTime * returnSmoothMultiplier);

        // Si estamos prácticamente en el objetivo, clava y para para evitar vibración
        float errNow = Mathf.DeltaAngle(current, targetAngle);
        if (Mathf.Abs(errNow) <= lockEpsilonDeg && Mathf.Abs(vel) <= lockVelEpsilon)
        {
            vel = 0f;
            rb.angularVelocity = Vector3.zero;
            return;
        }


        float next = Mathf.SmoothDampAngle(current, targetAngle, ref vel, st, maxSpeedDeg, Time.fixedDeltaTime);

        // Snap final para que no oscile alrededor del objetivo
        float errNext = Mathf.DeltaAngle(next, targetAngle);
        if (Mathf.Abs(errNext) <= lockEpsilonDeg)
        {
            next = targetAngle;
            vel = 0f;
        }


        // Aplicamos SOLO el delta alrededor del eje del joint
        float delta = next - current;

        // Si el delta es muy pequeño, no hacemos nada (evita micro-jitter)
        if (Mathf.Abs(delta) < 0.001f)
            return;

        Vector3 axisWorld = transform.TransformDirection(hinge.axis).normalized;
        Quaternion q = Quaternion.AngleAxis(delta, axisWorld);

        rb.MoveRotation(q * rb.rotation);
    }

    public void ForceResetImmediate()
    {
        targetPulled = false;
        targetAngle = restAngle;
        vel = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.rotation = restWorldRotation;
            rb.Sleep();
        }

        // Por si el rigidbody no actualiza visual inmediato en ese frame
        transform.rotation = restWorldRotation;
    }



}
