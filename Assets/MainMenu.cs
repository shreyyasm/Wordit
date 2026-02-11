using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text;

public class MainMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text")]
    public TextMeshProUGUI titleText;

    [Header("Parent Image (Hover Target)")]
    public Image parentImage;

    [Header("Scramble Settings")]
    public float scrambleSpeed = 0.05f;

    private string originalText;
    private Color originalImageColor;
    private Tween scrambleTween;

    private const string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    void Awake()
    {
        originalText = titleText.text;

        // Auto-grab parent Image if not assigned
        if (!parentImage)
            parentImage = titleText.GetComponentInParent<Image>();

        originalImageColor = parentImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScramble();
        parentImage.color = Color.white; // hover → white
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopScramble();
        parentImage.color = originalImageColor; // restore
    }

    void StartScramble()
    {
        scrambleTween?.Kill();

        scrambleTween = DOVirtual.DelayedCall(
            scrambleSpeed,
            () => titleText.text = ScrambleText(originalText),
            true
        ).SetLoops(-1);
    }

    void StopScramble()
    {
        scrambleTween?.Kill();
        titleText.text = originalText;
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