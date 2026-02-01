using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockedColumn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI letterText;
    [SerializeField] Image background;

    bool isSolved = false;

    public void Init(char letter)
    {
        letterText.text = letter.ToString();
        ApplyNormalColors();
    }

    void ApplyNormalColors()
    {
        letterText.color = ColorSchemeManager.Current.Letter;
        background.color = ColorSchemeManager.Current.letterBG;
    }

    public void SetSolvedState()
    {
        isSolved = true;

        background.color =
            ColorSchemeManager.Current.solvedCenterLetter;
    }
    public void PlaySolvedSqueeze()
    {
        RectTransform rt = background.rectTransform;

        rt.localScale = Vector3.one;

        rt.DOScaleY(0.5f, 0.12f)
    .SetEase(Ease.OutQuad)
    .OnComplete(() =>
    {
        rt.DOScaleY(1f, 0.14f)
          .SetEase(Ease.OutCubic);
    });

    }


}
