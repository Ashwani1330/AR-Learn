using UnityEngine;

public class KukaCcdIkSolver : MonoBehaviour
{
    [System.Serializable]
    public class SolverJoint
    {
        public Transform pivot;
        public Vector3 localAxis;
        public float minDeg;
        public float maxDeg;

        [HideInInspector] public Quaternion restRotation;
        [HideInInspector] public float angleDeg;
    }

    public KukaArticulationMotion motion;
    public Transform target;
    public Transform tcp;

    public SolverJoint j1 = new SolverJoint();
    public SolverJoint j2 = new SolverJoint();
    public SolverJoint j3 = new SolverJoint();
    public SolverJoint j4 = new SolverJoint();
    public SolverJoint j5 = new SolverJoint();
    public SolverJoint j6 = new SolverJoint();

    public int maxIterations = 30;
    public float positionTolerance = 0.005f;
    public float maxStepPerJointDeg = 8f;

    private void Awake()
    {
        SaveRest(j1);
        SaveRest(j2);
        SaveRest(j3);
        SaveRest(j4);
        SaveRest(j5);
        SaveRest(j6);
    }

    [ContextMenu("Solve once and move")]
    public void SolveOnceAndMove()
    {
        if (motion  == null || target == null || tcp == null)
            return;

        SyncFromMotion();
        ApplyAll();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            float err = Vector3.Distance(tcp.position, target.position);
            if (err < positionTolerance)
            {
                Debug.LogWarning("yoink!!!");
                break;
            }

            SolveJoint(j6);
            SolveJoint(j5);
            SolveJoint(j4);
            SolveJoint(j3);
            SolveJoint(j2);
            SolveJoint(j1);
        }

        ApplyAll();

        motion.MoveToAngles(new float[]
        {
            j1.angleDeg,
            j2.angleDeg,
            j3.angleDeg,
            j4.angleDeg,
            j5.angleDeg,
            j6.angleDeg
        });
    }

    void SyncFromMotion()
    {
        float[] a = motion.GetCurrentAnglesCopy();
        if (a == null || a.Length < 6) return;

        j1.angleDeg = a[0];
        j2.angleDeg = a[1];
        j3.angleDeg = a[2];
        j4.angleDeg = a[3];
        j5.angleDeg = a[4];
        j6.angleDeg = a[5];
    }

    void SolveJoint(SolverJoint j)
    {
        if (j == null || j.pivot == null || target == null || tcp == null)
            return;

        Vector3 axisWorld = j.pivot.TransformDirection(j.localAxis.normalized);
        Vector3 toTcp = Vector3.ProjectOnPlane(tcp.position - j.pivot.position, axisWorld);
        Vector3 toTarget = Vector3.ProjectOnPlane(target.position - j.pivot.position, axisWorld);

        if (toTcp.sqrMagnitude < 1e-10f || toTarget.sqrMagnitude < 1e-10f)
            return;

        float delta = Vector3.SignedAngle(toTcp, toTarget, axisWorld);
        delta = Mathf.Clamp(delta, -maxStepPerJointDeg, maxStepPerJointDeg);

        j.angleDeg = Mathf.Clamp(j.angleDeg + delta, j.minDeg, j.maxDeg);

        Apply(j);
    }

    void ApplyAll()
    {
        Apply(j1);
        Apply(j2);
        Apply(j3);
        Apply(j4);
        Apply(j5);
        Apply(j6);
    }

    void Apply(SolverJoint j)
    {
        if (j == null || j.pivot == null)
            return;

        j.pivot.localRotation = j.restRotation * Quaternion.AngleAxis(j.angleDeg, j.localAxis.normalized);
    }
    
    void SaveRest(SolverJoint j)
    {
        if (j == null || j.pivot == null)
            return;

        j.restRotation = j.pivot.localRotation;
    }
}
