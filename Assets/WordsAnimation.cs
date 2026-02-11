using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text;

public class wordsAnimation : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI[] titleText;

    [Header("Parent Images (Hover Target)")]
    public Image[] parentImage;
    public Image[] ArrowImage;

    [Header("Scramble Settings")]
    public float scrambleSpeed = 0.05f;

    private string[] originalTexts;
    private Color[] originalImageColors;
    private Tween scrambleTween;

    private const string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    [Header("Loop Image Animation")]
    public Image loopImage;
    public float loopSpeed = 2f;   // higher = faster
    public float rightBuffer = 100f; // extra distance past right edge
    void Awake()
    {
        // Cache original texts
        originalTexts = new string[titleText.Length];
        for (int i = 0; i < titleText.Length; i++)
            originalTexts[i] = titleText[i].text;

        // Cache original image colors
        originalImageColors = new Color[parentImage.Length];
        for (int i = 0; i < parentImage.Length; i++)
            originalImageColors[i] = parentImage[i].color;

        StartLoopImage();
    }
    public void StartLoopImage()
    {
        if (!loopImage) return;

        RectTransform rt = loopImage.rectTransform;
        RectTransform canvasRT = loopImage.canvas.GetComponent<RectTransform>();

        float canvasWidth = canvasRT.rect.width;
        float imageWidth = rt.rect.width;

        float startX = -canvasWidth / 2f - imageWidth + -rightBuffer;
        float endX = canvasWidth / 2f + imageWidth + rightBuffer;

        rt.anchoredPosition = new Vector2(startX, rt.anchoredPosition.y);

        rt.DOAnchorPosX(endX, loopSpeed)
          .SetEase(Ease.Linear)
          .SetLoops(-1, LoopType.Restart);
    }
    public void OnPointerEnter()
    {
        StartScramble();

        // All images → white
        for (int i = 0; i < parentImage.Length; i++)
            parentImage[i].color = Color.white;

        // Restore all image colors
        for (int i = 0; i < ArrowImage.Length; i++)
            ArrowImage[i].color = originalImageColors[i];
    }

    public void OnPointerExit()
    {
        StopScramble();

        // Restore all image colors
        for (int i = 0; i < parentImage.Length; i++)
            parentImage[i].color = originalImageColors[i];

        // Restore all image colors
        for (int i = 0; i < ArrowImage.Length; i++)
            ArrowImage[i].color = Color.white;
    }

    void StartScramble()
    {
        scrambleTween?.Kill();

        scrambleTween = DOVirtual.DelayedCall(
            scrambleSpeed,
            () =>
            {
                for (int i = 0; i < titleText.Length; i++)
                    titleText[i].text = ScrambleText(originalTexts[i]);
            },
            true
        ).SetLoops(-1);
    }

    void StopScramble()
    {
        scrambleTween?.Kill();

        for (int i = 0; i < titleText.Length; i++)
            titleText[i].text = originalTexts[i];
    }

    string ScrambleText(string input)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == ' ')
                sb.Append(' ');
            else
                sb.Append(randomChars[Random.Range(0, randomChars.Length)]);
        }

        return sb.ToString();
    }
}