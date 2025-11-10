using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefractionRay : MonoBehaviour
{
    public float nGlass = 1.5f; // Refractive index of glass (adjust as needed)
    public Vector3 startPosition = new Vector3(0f, 1.5f, 0f); // Ray starting point (above slab)
    public Vector3 incidentDirection = new Vector3(-0.5f, -1f, 0f); // Near 45-degree downward direction (will be normalized)
    public Transform slabTransform; // Assign the slab GameObject here
    public Slider refractiveIndexSlider;
    public TextMeshProUGUI refractiveIndexLabel;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // --- Auto-assign slabTransform ---
        if (slabTransform == null)
        {
            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            if (mr != null)
                slabTransform = mr.transform;
        }

        if (refractiveIndexSlider == null)
            refractiveIndexSlider = FindObjectOfType<Slider>();

        if (refractiveIndexLabel == null)
            refractiveIndexLabel = FindObjectOfType<TextMeshProUGUI>();

        if (refractiveIndexSlider != null)
        {
            refractiveIndexSlider.value = nGlass;
            refractiveIndexSlider.onValueChanged.AddListener((value) => { nGlass = value; });
        }

        if (slabTransform != null)
        {
            Bounds b = slabTransform.GetComponent<MeshRenderer>().bounds;

            // 1️⃣ Start just above the slab
            Vector3 origin = new Vector3(b.center.x, b.max.y + 0.5f, b.center.z);

            // 2️⃣ Normalize incident direction
            Vector3 dir = incidentDirection.normalized;

            // 3️⃣ Find where the ray *would* hit the top plane (y = b.max.y)
            if (dir.y < 0)
            {
                float t = (b.max.y - origin.y) / dir.y;
                Vector3 entry = origin + t * dir;

                // If the entry is OUTSIDE x-z bounds, shift origin sideways so it enters
                if (entry.x < b.min.x || entry.x > b.max.x || entry.z < b.min.z || entry.z > b.max.z)
                {
                    // Shift origin so projected entry lands at cube center
                    float verticalDrop = origin.y - b.max.y;
                    float horizShiftX = (-dir.x / dir.y * verticalDrop) - 0.05f;
                    float horizShiftZ = -dir.z / dir.y * verticalDrop;

                    origin.x = b.center.x - horizShiftX;
                    origin.z = b.center.z - horizShiftZ;
                }
            }

            startPosition = origin; // ✅ auto-adjusted
        }
    }


    void Update()
    {
        CalculateRay();

        if (refractiveIndexLabel)
        {
            refractiveIndexLabel.text = $"Refractive Index: {nGlass:F2}";
        }
    }

    void CalculateRay()
    {
        if (slabTransform == null) return;

        Bounds bounds = slabTransform.GetComponent<MeshRenderer>().bounds;
        float minY = bounds.min.y;
        float maxY = bounds.max.y;
        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;

        Vector3 origin = startPosition;
        Vector3 dir = incidentDirection.normalized;

        // Skip if not moving toward slab (downward)
        if (dir.y >= 0) return;

        // Calculate entry point on top face (y = maxY)
        float t1 = (maxY - origin.y) / dir.y;
        if (t1 < 0) return;
        Vector3 entry = origin + t1 * dir;

        // Check if entry is within slab's x-z bounds
        if (entry.x < minX || entry.x > maxX || entry.z < minZ || entry.z > maxZ) return;

        // Refraction: air to glass
        Vector3 N_entry = new Vector3(0f, 1f, 0f); // Outward normal for top face
        Vector3 refr_dir = Refract(dir, N_entry, nGlass);
        if (refr_dir == Vector3.zero) return; // Total internal reflection (rare for entry)
        refr_dir = refr_dir.normalized;

        // Calculate exit point on bottom face (y = minY)
        float t2 = (minY - entry.y) / refr_dir.y;
        if (t2 < 0) return;
        Vector3 exit = entry + t2 * refr_dir;

        // Check if exit is within slab's x-z bounds (should be for parallel faces)
        if (exit.x < minX || exit.x > maxX || exit.z < minZ || exit.z > maxZ) return;

        // Refraction: glass to air
        Vector3 N_exit = new Vector3(0f, -1f, 0f); // Outward normal for bottom face
        Vector3 out_dir = Refract(refr_dir, N_exit, nGlass);
        if (out_dir == Vector3.zero) return; // TIR
        out_dir = out_dir.normalized;

        // Extend the outgoing ray for visualization (adjust distance as needed)
        Vector3 end = exit + 2f * out_dir;

        // Set LineRenderer positions: start -> entry -> exit -> end
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, entry);
        lineRenderer.SetPosition(2, exit);
        lineRenderer.SetPosition(3, end);
    }

    private Vector3 Refract(Vector3 I, Vector3 N, float ior)
    {
        I = I.normalized;
        N = N.normalized;

        float cosi = Vector3.Dot(I, N);
        float etai = 1f, etat = ior;
        Vector3 n = N;

        if (cosi < 0)
        {
            cosi = -cosi;
        }
        else
        {
            float temp = etai;
            etai = etat;
            etat = temp;
            n = -N;
        }

        float eta = etai / etat;
        float k = 1f - eta * eta * (1f - cosi * cosi);

        if (k < 0) return Vector3.zero;

        return eta * I + (eta * cosi - Mathf.Sqrt(k)) * n;
    }
}
