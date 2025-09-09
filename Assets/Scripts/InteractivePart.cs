using UnityEngine;
using System.Collections.Generic; // Required to use Lists

public class InteractivePart : MonoBehaviour
{
    public string partName;
    private APIManager apiManager;

    [SerializeField]
    private Color highlightColor = Color.yellow;

    private MeshRenderer meshRenderer;
    private List<Color> originalColors = new List<Color>();
    private bool isHighlighted = false;

    // A static variable to track the currently highlighted part across all instances
    private static InteractivePart currentlyHighlighted;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // At the start, loop through all materials and save each original color
            foreach (Material mat in meshRenderer.materials)
            {
                originalColors.Add(mat.color);
            }
        }
        apiManager = FindObjectOfType<APIManager>();
    }

    // This function will be called by the 'Select' (tap) event
    public void OnPartSelected()
    {
        // If another part is already highlighted, un-highlight it first
        if (currentlyHighlighted != null && currentlyHighlighted != this)
        {
            currentlyHighlighted.Unhighlight();
        }

        // Toggle the highlight state for this part
        if (!isHighlighted)
        {
            Highlight();
        }
        else
        {
            Unhighlight();
        }

        if (apiManager != null)
        {
            apiManager.SetPartContext(partName);
        }
    }

    private void Highlight()
    {
        if (meshRenderer == null) return;

        // Loop through all materials and apply the highlight color and glow
        foreach (Material mat in meshRenderer.materials)
        {
            mat.color = highlightColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", highlightColor);
        }
        isHighlighted = true;
        currentlyHighlighted = this;
    }

    private void Unhighlight()
    {
        if (meshRenderer == null) return;

        // Loop through all materials and restore their original colors
        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            meshRenderer.materials[i].color = originalColors[i];
            meshRenderer.materials[i].DisableKeyword("_EMISSION");
        }
        isHighlighted = false;
        currentlyHighlighted = null;
    }
}