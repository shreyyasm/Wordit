using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    [SerializeField] Image background;

    Color defaultColor;

    void Awake()
    {
        defaultColor = background.color;
    }

    public void SetSolvedColor()
    {
        background.color =
            ColorSchemeManager.Current.solvedCenterLetter;
    }

    public void ResetColor()
    {
        background.color = defaultColor;
    }
}