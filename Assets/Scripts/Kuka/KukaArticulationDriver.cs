using UnityEngine;

public class KukaArticulationDriver : MonoBehaviour
{
    [System.Serializable]
    public class JointTarget
    {
        public ArticulationBody joint;
        public float targetDeg;
    }

    public JointTarget j1;
    public JointTarget j2;
    public JointTarget j3;
    public JointTarget j4;
    public JointTarget j5;
    public JointTarget j6;

    void Update()
    {
        Apply(j1);
        Apply(j2);
        Apply(j3);
        Apply(j4);
        Apply(j5);
        Apply(j6);
    }

    void Apply(JointTarget jt)
    {
        if (jt == null || jt.joint == null) return;

        var drive = jt.joint.xDrive;
        float clamped = Mathf.Clamp(jt.targetDeg, drive.lowerLimit, drive.upperLimit);
        drive.target = clamped;
        jt.joint.xDrive = drive;
    }
}
