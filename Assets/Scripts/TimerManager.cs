using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public WinTransition winTransition;
    public TextMeshProUGUI timerText;
    public Image timerFill; // <-- center shrinking bar

    float time;
    float maxTime;

    GameWordManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameWordManager>();
    }

    public void StartTimer(float t)
    {
        maxTime = t;
        time = t;
        UpdateUI();
    }

    void Update()
    {
        if (gm.levelSolved) return;
        if (time <= 0)
            return;

        time -= Time.deltaTime;
        time = Mathf.Max(time, 0);

        UpdateUI();

        if (time <= 0)
        {
            winTransition.PlayLose(() =>
            {
                gm.Lose();
            });
        }
          
    }

    public void AddTime(float seconds)
    {
        time += seconds;
        UpdateUI();
    }
    float displayedFill = 1f;
    void UpdateUI()
    {
        timerText.text = Mathf.Ceil(time).ToString();

        float targetFill = time / maxTime;

        displayedFill = Mathf.Lerp(
            displayedFill,
            targetFill,
            Time.deltaTime * 10f   // ← smoothness control
        );

        timerFill.rectTransform.localScale =
            new Vector3(displayedFill, 1f, 1f);
    }
}