using DG.Tweening;
using System.Collections;
using UnityEngine;

public class WinTransition : MonoBehaviour
{
    [Header("References")]
    public RectTransform winImage;
    public GameObject streakParent;

    [Header("Timings")]
    public float startDelay = 0.6f;
    public float growXDuration = 0.25f;
    public float waitBeforeExpand = 1f;
    public float expandYDuration = 0.25f;

    Coroutine playRoutine;
    Sequence activeSequence;

    void Awake()
    {
        winImage.gameObject.SetActive(false);
    }

    public void Play(System.Action onComplete)
    {
        // 🔥 stop any previous transition cleanly
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        if (activeSequence != null && activeSequence.IsActive())
            activeSequence.Kill();

        playRoutine = StartCoroutine(PlayDelayed(onComplete));
    }

    IEnumerator PlayDelayed(System.Action onComplete)
    {
        yield return new WaitForSeconds(startDelay);
        PlayInternal(onComplete);
    }

    void PlayInternal(System.Action onComplete)
    {
        winImage.gameObject.SetActive(true);
        streakParent.SetActive(true);

        winImage.localScale = new Vector3(0f, 1f, 1f);

        activeSequence = DOTween.Sequence();

        // 1️⃣ grow horizontally
        activeSequence.Append(
            winImage.DOScaleX(1f, growXDuration)
                    .SetEase(Ease.OutBack)
        );

        // 2️⃣ wait
        activeSequence.AppendInterval(waitBeforeExpand);

        // 3️⃣ expand vertically
        activeSequence.Append(
            winImage.DOScaleY(20f, expandYDuration)
                    .SetEase(Ease.InOutQuad)
        );

        // 4️⃣ finish
        activeSequence.OnComplete(() =>
        {
            winImage.gameObject.SetActive(false);
            activeSequence = null;
            onComplete?.Invoke();
        });
    }

    public void DelaodStreak()
    {
        streakParent.SetActive(false);
    }
}