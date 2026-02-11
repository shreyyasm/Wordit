using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Panels")]
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public Canvas canvas;

    [Header("Animation")]
    public float duration = 0.4f;
    public Ease ease = Ease.OutQuart;

    private float moveDistance;

    private void Awake()
    {
        Instance = this;

        // Get reference resolution height
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        float referenceHeight = scaler.referenceResolution.y;

        // push completely outside view
        moveDistance = referenceHeight / 2f + topPanel.rect.height / 2f;
    }

    // =========================
    // CLOSE → panels come IN
    // =========================
    public void Close(Action onComplete = null)
    {
        topPanel.DOKill();
        bottomPanel.DOKill();

        topPanel.anchoredPosition = new Vector2(0, moveDistance);
        bottomPanel.anchoredPosition = new Vector2(0, -moveDistance);

        DOTween.Sequence()
            .Join(topPanel.DOAnchorPosY(0, duration).SetEase(ease))
            .Join(bottomPanel.DOAnchorPosY(0, duration).SetEase(ease))
            .OnComplete(() => onComplete?.Invoke());
    }

    // =========================
    // OPEN → panels go OUT
    // =========================
    public void Open(Action onComplete = null)
    {
        topPanel.DOKill();
        bottomPanel.DOKill();

        DOTween.Sequence()
            .Join(topPanel.DOAnchorPosY(moveDistance, duration).SetEase(ease))
            .Join(bottomPanel.DOAnchorPosY(-moveDistance, duration).SetEase(ease))
            .OnComplete(() => onComplete?.Invoke());
    }
}