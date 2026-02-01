using UnityEngine;
using TMPro;

public class ColorSchemeManager : MonoBehaviour
{
    [Header("Color Schemes")]
    public ColorScheme[] schemes;

    [Header("Scene References")]
    public Camera mainCamera;

    static ColorScheme currentScheme;

    public static ColorScheme Current => currentScheme;

    public void ApplyRandomScheme()
    {
        if (schemes == null || schemes.Length == 0)
        {
            Debug.LogError("No color schemes assigned!");
            return;
        }

        currentScheme =
            schemes[Random.Range(0, schemes.Length)];

        ApplyScheme(currentScheme);
    }

    void ApplyScheme(ColorScheme scheme)
    {
        // 1️⃣ Background
        if (mainCamera != null)
            mainCamera.backgroundColor = scheme.background;
    }
}
