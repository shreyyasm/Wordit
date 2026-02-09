using UnityEngine;
using DG.Tweening;
using System;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    [Header("Panels")]
    public RectTransform topPanel;
    public RectTransform bottomPanel;

    [Header("Animation")]
    public float moveDistance = 600f;
    public float duration = 0.4f;
    public Ease ease = Ease.OutQuart;

    private void Awake()
    {
        Instance = this;
    }
    // =========================
    // CLOSE → panels go OUT
    // =========================
    public void Close(Action onComplete = null)
    {
      
        

        topPanel.DOKill();
        bottomPanel.DOKill();

        // start offscreen
        topPanel.anchoredPosition = new Vector2(0, moveDistance);
        bottomPanel.anchoredPosition = new Vector2(0, -moveDistance);

        Sequence seq = DOTween.Sequence();

        // both come to center at same time
        seq.Join(
            topPanel.DOAnchorPosY(0f, duration)
                    .SetEase(ease)
        );

        seq.Join(
            bottomPanel.DOAnchorPosY(0f, duration)
                       .SetEase(ease)
        );

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    // =========================
    // OPEN → panels come IN
    // =========================
    public void Open(Action onComplete = null)
    {

        topPanel.DOKill();
        bottomPanel.DOKill();

        // start from center
        topPanel.anchoredPosition = Vector2.zero;
        bottomPanel.anchoredPosition = Vector2.zero;

        Sequence seq = DOTween.Sequence();

        // top goes UP
        seq.Join(
            topPanel.DOAnchorPosY(moveDistance, duration)
                    .SetEase(ease)
        );

        // bottom goes DOWN
        seq.Join(
            bottomPanel.DOAnchorPosY(-moveDistance, duration)
                       .SetEase(ease)
        );

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}