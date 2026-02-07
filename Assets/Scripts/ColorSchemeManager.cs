using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorSchemeManager : MonoBehaviour
{
    [SerializeField] public Image BackgroundUI;

    [Header("Color Schemes")]
    public ColorScheme[] schemes;

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
        
           BackgroundUI.color = scheme.background;
    }
}
