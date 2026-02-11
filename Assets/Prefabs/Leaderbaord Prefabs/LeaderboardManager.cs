using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static DevvitBridge;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject leaderbaordCanvas;
    public Transform contentRoot;
    public Transform BottomContentPrefab;
    public LeaderboardRowUI rowPrefab;
    public LeaderboardRowUI separatorPrefab;
    public GameObject emptyText;

    private const string INIT_URL = "/api/init";
    private const string LEADERBOARD_URL = "/api/leaderboard";
    private const string USER_RANK_URL = "/api/user-rank";

    void Start()
    {
        StartCoroutine(InitThenLoadLeaderboard());
    }

    IEnumerator InitThenLoadLeaderboard()
    {
        UnityWebRequest req = UnityWebRequest.Get(INIT_URL);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Init failed: " + req.error);
            yield break;
        }

        Debug.Log("[INIT RAW] " + req.downloadHandler.text);

        InitResponse init =
            JsonUtility.FromJson<InitResponse>(req.downloadHandler.text);

        if (!string.IsNullOrEmpty(init.username))
        {
            PlayerPrefs.SetString("username", init.username);
            PlayerPrefs.Save();
            Debug.Log("Username initialized: " + init.username);
        }

        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        ClearUI();
        if (emptyText) emptyText.SetActive(false);
        StartCoroutine(LoadLeaderboardRoutine());
    }

    private IEnumerator LoadLeaderboardRoutine()
    {
        // -------- TOP 10 --------
        UnityWebRequest topReq = UnityWebRequest.Get(LEADERBOARD_URL);
        yield return topReq.SendWebRequest();

        if (topReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Leaderboard error: " + topReq.error);
            ShowEmptyState();
            yield break;
        }

        Debug.Log("[LEADERBOARD RAW] " + topReq.downloadHandler.text);

        LeaderboardResponse leaderboard =
            JsonUtility.FromJson<LeaderboardResponse>(topReq.downloadHandler.text);

        if (leaderboard == null || leaderboard.leaderboard == null || leaderboard.leaderboard.Length == 0)
        {
            ShowEmptyState();
            yield break;
        }

        bool userInTop10 = false;
        string localUsername = GetLocalUsername();

        foreach (var entry in leaderboard.leaderboard)
        {
            LeaderboardRowUI row = Instantiate(rowPrefab, contentRoot);
            row.SetData(GetRankDisplay(entry.rank), entry.username, entry.score);

            if (entry.username == localUsername)
            {
                row.Highlight(true);
                userInTop10 = true;
            }
          
        }

        // -------- USER ROW --------
        if (!userInTop10)
        {
            UnityWebRequest userReq = UnityWebRequest.Get(USER_RANK_URL);
            yield return userReq.SendWebRequest();

            if (userReq.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[USER RANK RAW] " + userReq.downloadHandler.text);

                UserRankResponse user =
                    JsonUtility.FromJson<UserRankResponse>(userReq.downloadHandler.text);

                if (user != null && user.rank > 10 && !string.IsNullOrEmpty(user.username))
                {
               
                    LeaderboardRowUI row = Instantiate(separatorPrefab, BottomContentPrefab);
                    row.SetData(GetRankDisplay(user.rank), user.username, user.score);
                    row.Highlight(true);
               
                }
            }
        }
    }

    void ShowEmptyState()
    {
        if (emptyText)
            emptyText.SetActive(true);
    }

    private void ClearUI()
    {
        if (!contentRoot)
        {
            Debug.LogError("ContentRoot is NULL");
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    private string GetLocalUsername()
    {
        return PlayerPrefs.GetString("username", "anonymous");
    }

    private string GetRankDisplay(int rank)
    {
        switch (rank)
        {
            case 1: return "🥇";
            case 2: return "🥈";
            case 3: return "🥉";
            default: return rank.ToString();
        }
    }
    // ================== DTOs ==================

    [System.Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string username;
        public int score;
    }

    [System.Serializable]
    public class LeaderboardResponse
    {
        public LeaderboardEntry[] leaderboard;
    }

    [System.Serializable]
    public class UserRankResponse
    {
        public int rank;
        public string username;
        public int score;
    }
    public void OpenLeaderboard()
    {
        leaderbaordCanvas.SetActive(true);
    }
    public void CloseLeaderboard()
    {
        leaderbaordCanvas.SetActive(false);
    }

}