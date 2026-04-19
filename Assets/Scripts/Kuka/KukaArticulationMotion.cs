using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class KukaArticulationMotion : MonoBehaviour
{
    [System.Serializable]
    public class JointChannel
    {
        public ArticulationBody joint;
        public float homeDeg;
        public float poseDeg;
        public float maxSpeedDegPerSec = 60f;
        public float currentDeg;
    }

    public JointChannel j1 = new JointChannel();
    public JointChannel j2 = new JointChannel();
    public JointChannel j3 = new JointChannel();
    public JointChannel j4 = new JointChannel();
    public JointChannel j5 = new JointChannel();
    public JointChannel j6 = new JointChannel();

    public bool moveHomeOnStart = false;
    public float arriveToleranceDeg = 0.25f;

    public Key homeKey = Key.H;
    public Key poseKey = Key.M;
    public Key stopKey = Key.S;

    private readonly float[] goals = new float[6];
    private bool moving = false;

    void Start()
    {
        SyncDrivesToCurrentPose();
        SetGoalsFromCurrent();
        UpdateCurrentAngles();

        if (moveHomeOnStart)
            MoveToHome();
    }

    void Update()
    {
        UpdateCurrentAngles();

        if (WasPressed(homeKey))
            MoveToHome();

        if (WasPressed(poseKey))
            MoveToPose();

        if (WasPressed(stopKey))
            StopMotion();

        if (!moving)
            return;

        StepJoint(j1, 0);
        StepJoint(j2, 1);
        StepJoint(j3, 2);
        StepJoint(j4, 3);
        StepJoint(j5, 4);
        StepJoint(j6, 5);

        if (AllReached())
            moving = false;
    }

    [ContextMenu("Capture Current As Home")]
    public void CaptureCurrentAsHome()
    {
        UpdateCurrentAngles();

        j1.homeDeg = j1.currentDeg;
        j2.homeDeg = j2.currentDeg;
        j3.homeDeg = j3.currentDeg;
        j4.homeDeg = j4.currentDeg;
        j5.homeDeg = j5.currentDeg;
        j6.homeDeg = j6.currentDeg;
    }

    [ContextMenu("Capture Current As Pose")]
    public void CaptureCurrentAsPose()
    {
        UpdateCurrentAngles();

        j1.poseDeg = j1.currentDeg;
        j2.poseDeg = j2.currentDeg;
        j3.poseDeg = j3.currentDeg;
        j4.poseDeg = j4.currentDeg;
        j5.poseDeg = j5.currentDeg;
        j6.poseDeg = j6.currentDeg;
    }

    [ContextMenu("Move To Home")]
    public void MoveToHome()
    {
        goals[0] = j1.homeDeg;
        goals[1] = j2.homeDeg;
        goals[2] = j3.homeDeg;
        goals[3] = j4.homeDeg;
        goals[4] = j5.homeDeg;
        goals[5] = j6.homeDeg;
        moving = true;
    }

    [ContextMenu("Move To Pose")]
    public void MoveToPose()
    {
        goals[0] = j1.poseDeg;
        goals[1] = j2.poseDeg;
        goals[2] = j3.poseDeg;
        goals[3] = j4.poseDeg;
        goals[4] = j5.poseDeg;
        goals[5] = j6.poseDeg;
        moving = true;
    }

    [ContextMenu("Stop Motion")]
    public void StopMotion()
    {
        moving = false;
        SyncDrivesToCurrentPose();
        SetGoalsFromCurrent();
        UpdateCurrentAngles();
    }

    void StepJoint(JointChannel jc, int goalIndex)
    {
        if (jc == null || jc.joint == null)
            return;

        float current = ReadAngleDeg(jc.joint);
        jc.currentDeg = current;

        float next = Mathf.MoveTowards(
            current,
            goals[goalIndex],
            jc.maxSpeedDegPerSec * Time.deltaTime
        );

        var drive = jc.joint.xDrive;
        next = Mathf.Clamp(next, drive.lowerLimit, drive.upperLimit);
        drive.target = next;
        jc.joint.xDrive = drive;
    }

    bool AllReached()
    {
        return Reached(j1, 0) &&
               Reached(j2, 1) &&
               Reached(j3, 2) &&
               Reached(j4, 3) &&
               Reached(j5, 4) &&
               Reached(j6, 5);
    }

    bool Reached(JointChannel jc, int goalIndex)
    {
        if (jc == null || jc.joint == null)
            return true;

        float current = ReadAngleDeg(jc.joint);
        jc.currentDeg = current;

        return Mathf.Abs(current - goals[goalIndex]) <= arriveToleranceDeg;
    }

    void UpdateCurrentAngles()
    {
        UpdateCurrent(j1);
        UpdateCurrent(j2);
        UpdateCurrent(j3);
        UpdateCurrent(j4);
        UpdateCurrent(j5);
        UpdateCurrent(j6);
    }

    void UpdateCurrent(JointChannel jc)
    {
        if (jc == null || jc.joint == null)
            return;

        jc.currentDeg = ReadAngleDeg(jc.joint);
    }

    void SyncDrivesToCurrentPose()
    {
        SyncDrive(j1);
        SyncDrive(j2);
        SyncDrive(j3);
        SyncDrive(j4);
        SyncDrive(j5);
        SyncDrive(j6);
    }

    void SyncDrive(JointChannel jc)
    {
        if (jc == null || jc.joint == null)
            return;

        float current = ReadAngleDeg(jc.joint);
        jc.currentDeg = current;

        var drive = jc.joint.xDrive;
        drive.target = Mathf.Clamp(current, drive.lowerLimit, drive.upperLimit);
        jc.joint.xDrive = drive;
    }

    void SetGoalsFromCurrent()
    {
        goals[0] = j1.currentDeg;
        goals[1] = j2.currentDeg;
        goals[2] = j3.currentDeg;
        goals[3] = j4.currentDeg;
        goals[4] = j5.currentDeg;
        goals[5] = j6.currentDeg;
    }

    float ReadAngleDeg(ArticulationBody joint)
    {
        return joint.jointPosition[0] * Mathf.Rad2Deg;
    }

    public void MoveToAngles(float[] targetDeg)
    {
        if (targetDeg == null || targetDeg.Length < 6) return;

        goals[0] = ClampToDrive(j1, targetDeg[0]);
        goals[1] = ClampToDrive(j2, targetDeg[1]);
        goals[2] = ClampToDrive(j3, targetDeg[2]);
        goals[3] = ClampToDrive(j4, targetDeg[3]);
        goals[4] = ClampToDrive(j5, targetDeg[4]);
        goals[5] = ClampToDrive(j6, targetDeg[5]);

        moving = true;
    }

    public float[] GetCurrentAnglesCopy()
    {
        UpdateCurrentAngles();
        return new float[]
        {
            j1.currentDeg,
            j2.currentDeg,
            j3.currentDeg,
            j4.currentDeg,
            j5.currentDeg,
            j6.currentDeg,
        };
    }


    float ClampToDrive(JointChannel jc, float deg)
    {
        if (jc == null || jc.joint == null) return deg;
        var drive = jc.joint.xDrive;
        return Mathf.Clamp(deg, drive.lowerLimit, drive.upperLimit);
    }

    bool WasPressed(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    }
}
