using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    float time;
    GameWordManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameWordManager>();
    }

    public void StartTimer(float t)
    {
        time = t;
    }

    void Update()
    {
        time -= Time.deltaTime;
        timerText.text = Mathf.Ceil(time).ToString();

        if (time <= 0)
            gm.Lose();
    }
}
