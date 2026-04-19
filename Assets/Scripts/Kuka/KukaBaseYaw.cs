using UnityEngine;

public class KukaBaseYaw : MonoBehaviour
{
    public Transform j1Pivot;
    public Transform target;

    // For your setup this is probably local Y
    public Vector3 localAxis = Vector3.up;

    public float angleOffsetDeg = 0f;
    public bool invert = false;

    private Quaternion restRotation;
    private Vector3 referenceDirWorld;
    private bool initialized = false;

    void Start()
    {
        if (j1Pivot == null || target == null) return;

        restRotation = j1Pivot.localRotation;

        Vector3 dir = target.position - j1Pivot.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 1e-8f)
        {
            referenceDirWorld = dir.normalized;
            initialized = true;
        }
    }

    void LateUpdate()
    {
        if (!initialized || j1Pivot == null || target == null) return;

        Vector3 currentDir = target.position - j1Pivot.position;
        currentDir.y = 0f;

        if (currentDir.sqrMagnitude < 1e-8f) return;

        currentDir.Normalize();

        float deltaYaw = Vector3.SignedAngle(referenceDirWorld, currentDir, Vector3.up);

        if (invert)
            deltaYaw = -deltaYaw;

        deltaYaw += angleOffsetDeg;

        j1Pivot.localRotation =
            restRotation * Quaternion.AngleAxis(deltaYaw, localAxis.normalized);
    }
}