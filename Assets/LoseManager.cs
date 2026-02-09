using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class LoseManager : MonoBehaviour
{
    public static LoseManager instance;
 
    [Header("Texts")]
    public TMP_Text yourScoreText;   // string
    public TMP_Text expText;         // int
    public TMP_Text streakText;      // int

    [Header("Values")]
    public int expValue;
    public int streakValue;

    [Header("Timings")]
    public float scaleDuration = 0.35f;
    public float numberDuration = 0.4f;
    public float delayBetween = 0.25f;

    public GameObject LoseScreen;
    private void Awake()
    {
        if(instance == null)
           instance = this;
    }
    public void PlayLoseScreen()
    {
        // safety
        yourScoreText.gameObject.SetActive(false);
        expText.gameObject.SetActive(false);
        streakText.gameObject.SetActive(false);

        expValue = PlayerProgressionManager.SessionXP;
        streakValue = GameWordManager.Instance.CurrentLevel;
        StartCoroutine(PlayLoseSequence());
    }

    IEnumerator PlayLoseSequence()
    {
        yield return new WaitForSeconds(0.5f);

        // 🔥 Prepare scale BEFORE enabling
        LoseScreen.transform.DOKill();
        LoseScreen.transform.localScale = new Vector3(1f, 0f, 1f);

        // 🔥 Show screen
        LoseScreen.SetActive(true);

        // 🔥 Scale Y animation
        LoseScreen.transform
            .DOScaleY(1f, 0.4f)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.5f);

        // 1️⃣ YOUR SCORE (STRING)
        yield return ShowStringText(yourScoreText);

        yield return new WaitForSeconds(delayBetween);

        // 2️⃣ EXP
        yield return ShowIntText(expText, expValue, "+", "XP");

        yield return new WaitForSeconds(delayBetween);

        // 3️⃣ STREAK
        yield return ShowIntText(streakText, streakValue, "<sprite=0>", "");
    }

    IEnumerator ShowStringText(TMP_Text text)
    {
        text.gameObject.SetActive(true);

        AnimateScale(text.transform);

        yield return new WaitForSeconds(scaleDuration);
    }

    IEnumerator ShowIntText(
     TMP_Text text,
     int targetValue,
     string prefix = "",
     string suffix = ""
 )
    {
        text.gameObject.SetActive(true);

        int shown = 0;
        text.text = $"{prefix}0{suffix}";

        AnimateScale(text.transform);

        DOTween.Kill(text);

        DOTween.To(() => shown, x =>
        {
            shown = x;
            text.text = $"{prefix}{shown}{suffix}";
        }, targetValue, numberDuration)
        .SetEase(Ease.OutExpo);

        yield return new WaitForSeconds(Mathf.Max(scaleDuration, numberDuration));
    }

    void AnimateScale(Transform t)
    {
        t.DOKill();
        t.localScale = Vector3.one * 1.6f;

        t.DOScale(1f, scaleDuration)
         .SetEase(Ease.OutBack);
    }
    public void CloseScree(System.Action onComplete)
    {
        LoseScreen.transform.DOKill();

        LoseScreen.transform.localScale = new Vector3(1f, 1f, 1f);

        LoseScreen.transform
            .DOScaleY(0f, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                LoseScreen.SetActive(false);
                onComplete?.Invoke(); // ✅ CALL THE CALLBACK
            });
    }
}