using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text scoreText;

    public void SetData(string rank, string username, int score)
    {
        rankText.text = rank;
        usernameText.text = username;
        scoreText.text = FormatScore(score);
    }

    public void Highlight(bool enabled)
    {
        if (enabled)
        {
            usernameText.color = Color.yellow;
            scoreText.color = Color.yellow;
        }
    }
    string FormatScore(int score)
    {
        if (score < 1000)
            return score.ToString();

        if (score < 1_000_000)
            return (score / 1000f).ToString("0.#") + "K";

        if (score < 1_000_000_000)
            return (score / 1_000_000f).ToString("0.#") + "M";

        return (score / 1_000_000_000f).ToString("0.#") + "B";
    }
}