using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchInputInitializer : MonoBehaviour
{
    private void OnEnable()
    {
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
            Debug.Log("[AR] EnhancedTouch enabled");
        }
    }

    private void OnDisable()
    {
        if (EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Disable();
            Debug.Log("[AR] EnhancedTouch disabled");
        }
    }
}
