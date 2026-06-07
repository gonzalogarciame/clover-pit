using UnityEngine;

public class LeverSpinTrigger : MonoBehaviour
{
    [SerializeField] private SlotMachineGrid slotMachineGrid;
    [SerializeField] private HingeJoint hinge;

    [Header("Positive pull angles")]
    [SerializeField] private float triggerAngle = 40f; // dispara al cruzar hacia abajo >= 40
    [SerializeField] private float rearmAngle = 10f;   // rearma al volver <= 10

    [Header("Safety")]
    [SerializeField] private float cooldown = 0.25f;



    private float prevAngle;
    private bool armed = true;
    private float nextAllowedTime = 0f;

    [System.Obsolete]
    void Awake()
    {
        if (!hinge) hinge = GetComponent<HingeJoint>();
        if (!slotMachineGrid) slotMachineGrid = FindObjectOfType<SlotMachineGrid>();
        prevAngle = hinge ? hinge.angle : 0f;
    }

    void Update()
    {
        if (!hinge || !slotMachineGrid) return;

        float a = hinge.angle;

        // Rearm solo cuando vuelve cerca de 0
        if (!armed && a <= rearmAngle)
            armed = true;

        // Dispara solo al cruzar hacia abajo: de < trigger a >= trigger
        bool crossedDown = prevAngle < triggerAngle && a >= triggerAngle;

        if (armed && crossedDown && Time.time >= nextAllowedTime)
        {
            armed = false;
            nextAllowedTime = Time.time + cooldown;

            if (!slotMachineGrid.IsSpinning)
                slotMachineGrid.Spin();
        }

        prevAngle = a;
    }
}
