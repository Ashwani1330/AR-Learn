using UnityEngine;

public class KukaJointDriver : MonoBehaviour
{
    [System.Serializable]
    public class Joint
    {
        public Transform pivot;
        public Vector3 LocalAxis = Vector3.up;
        public float angleDeg;
        [HideInInspector] public Quaternion restRotation;
    }

    public Joint j1 = new Joint();
    public Joint j2 = new Joint();
    public Joint j3 = new Joint();
    public Joint j4 = new Joint();
    public Joint j5 = new Joint();
    public Joint j6 = new Joint();

    private void Awake()
    {
        SaveRest(j1);
        SaveRest(j2);
        SaveRest(j3);
        SaveRest(j4);
        SaveRest(j5);
        SaveRest(j6); 
    } 

    private void LateUpdate()
    {
        Apply(j1);
        Apply(j2);
        Apply(j3);
        Apply(j4);
        Apply(j5);
        Apply(j6);
    }

    void SaveRest(Joint j)
    {
        if (j != null && j.pivot != null)
            j.restRotation = j.pivot.localRotation;
    }

    void Apply(Joint j)
    {
        if (j == null || j.pivot == null) return;

        Vector3 axis = j.LocalAxis.normalized;
        j.pivot.localRotation = j.restRotation * Quaternion.AngleAxis(j.angleDeg, axis);
    }
}
