using UnityEngine;
using System.Collections.Generic; // Required to use Lists

public class InteractivePart : MonoBehaviour
{
    public string partName;

    [SerializeField]
    private Color highlightColor = Color.yellow;

    private MeshRenderer meshRenderer;
    // Store a list of original colors for all materials
    private List<Color> originalColors = new List<Color>();

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Loop through all materials and save each original color
            foreach (Material mat in meshRenderer.materials)
            {
                originalColors.Add(mat.color);
            }
        }
    }

    public void OnPartHovered()
    {
        if (meshRenderer != null)
        {
            // Loop through all materials and apply the highlight
            foreach (Material mat in meshRenderer.materials)
            {
                mat.color = highlightColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", highlightColor);
            }
        }
    }

    public void OnPartUnhovered()
    {
        if (meshRenderer != null)
        {
            // Loop through all materials and restore their original colors
            for (int i = 0; i < meshRenderer.materials.Length; i++)
            {
                meshRenderer.materials[i].color = originalColors[i];
                meshRenderer.materials[i].DisableKeyword("_EMISSION");
            }
        }
    }

    public void OnPartSelected()
    {
        Debug.Log(partName + " was selected! This will trigger the AI tutor.");
    }
}