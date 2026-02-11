using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro; // If using TextMeshPro
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI; // For Image component

// This script is used to communicate with the Devvit API and update the UI with the data from the API.
// This shows how to:
// 1. Pull in the username and snoovatar image from Reddit
// 2. Pull in a level index so you can alter the Unity level based on Reddit post information
// 3. Send level completed time out of Unity back to the Reddit server so you can store your data in Redis

public class DevvitBridge : MonoBehaviour
{
    public static DevvitBridge Instance;
    [Header("UI References")]
    public TMP_Text usernameText;
    public Image targetImage;

    public TMP_Text previousTimeText;


    // Store the fetched data
    private string currentUsername;
    private string currentPostId;

    public TextMeshProUGUI playerScoreText;
    public int playerScore;

    public TextMeshProUGUI globalScoreText;
    public int globalScore;

    // API Response Classes (must match JSON structure listed in src/shared/types/api.ts)
    [System.Serializable]
    public class InitResponse
    {
        public string type;
        public string postId;
        public string username;
        public string snoovatarUrl;
        public string previousTime; // will be an empty string if no previous time exists
        public int score;
        public string globalscore;
    }

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

        // Fetch initial data when the game starts
        StartCoroutine(FetchInitData());
       
    }

    // GET request to /api/init - Fetches username, previous time, and avatar
    public IEnumerator FetchInitData()
    {

        UnityWebRequest request = UnityWebRequest.Get("/api/init");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Error fetching init data: " + request.error + " — this will occur when running in Unity.");

            yield break;
        }

        // Parse and store the data
        InitResponse data = JsonUtility.FromJson<InitResponse>(request.downloadHandler.text);

        if(usernameText != null)
        {
            // Set username
            currentUsername = data.username;
            usernameText.text = "u/" + currentUsername;

        }

        // Store post ID
        currentPostId = data.postId;

        //// Set previous time (if available)
        //if (previousTimeText != null && !string.IsNullOrEmpty(data.previousTime))
        //{
        //    previousTimeText.text = "Previous Time: " + data.previousTime + "s";
        //}


        //// Set previous score (if available)
        //if (playerScoreText != null && !string.IsNullOrEmpty(data.score))
        //{
        //    playerScore = int.Parse(data.score);
        //    playerScoreText.text = "playerScore:  " + data.score;
        //}

        //// Set previous score (if available)
        //if (globalScoreText != null && !string.IsNullOrEmpty(data.globalscore))
        //{
        //    globalScore = int.Parse(data.globalscore);
        //    globalScoreText.text = "GlobalScore:  " + data.globalscore;
        //}

        // Download avatar image
        if (!string.IsNullOrEmpty(data.snoovatarUrl))
        {
            yield return StartCoroutine(DownloadImage(data.snoovatarUrl));
        }

    }

    // Downloads and displays the user's avatar image
    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading image: " + request.error);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        if (targetImage != null && texture != null)
        {
            // Convert texture to sprite and assign to Image
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = newSprite;
            targetImage.preserveAspect = true; // Prevent squishing
        }
    }

    // Sends level completion data to server. Accepts an optional callback invoked with true on success, false on failure.
    public void CompleteLevel(float completionTime, Action<bool> onComplete = null)
    {
        StartCoroutine(SendCompletionRequest(completionTime, onComplete));
    }

    [System.Serializable]
    private class LevelCompletionData
    {
        public string type;
        public string username;
        public string postId;
        public string time; // Changed to string to match server expectations
    }

    private IEnumerator SendCompletionRequest(float completionTime, Action<bool> onComplete)
    {
        // Prepare request
        UnityWebRequest request = new UnityWebRequest("/api/level-completed", "POST");

        LevelCompletionData data = new()
        {
            type = "level-completed",
            username = currentUsername,
            postId = currentPostId,
            time = completionTime.ToString("F2"), // Convert to string with 2 decimal places
        };

        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;

        if (!success)
        {
            Debug.LogWarning("Error sending completion data: " + request.error + " — this will occur when running in Unity.");
        }
        else
        {
            Debug.Log("Score Saved sucksexfully");
        }


        // Invoke callback if provided
        try
        {
            onComplete?.Invoke(success);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error invoking completion callback: " + ex.Message);
        }
    }
    [System.Serializable]
    public class PostCreateResponse
    {
        public string status;
        public string message;
    }

    public void OnCreatePostButtonClicked()
    {
        StartCoroutine(SendPostCreateRequest(OnPostCreateCompleted));
    }
    private void OnPostCreateCompleted(bool success)
    {
        if (success)
        {
            Debug.Log("✅ Post created successfully");
        }
        else
        {
            Debug.LogError("❌ Failed to create post");
        }
    }

    private IEnumerator SendPostCreateRequest(Action<bool> onComplete)
    {
        // Prepare request
        UnityWebRequest request = new UnityWebRequest("/api/post-create", "POST");

        // No body required for this endpoint
        request.uploadHandler = new UploadHandlerRaw(new byte[0]);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;

        if (!success)
        {
            Debug.LogWarning(
                "Error creating post: " +
                request.error +
                " | Response: " +
                request.downloadHandler.text
            );
        }
        else
        {
            Debug.Log("Post created successfully: " + request.downloadHandler.text);
        }

        // Invoke callback safely
        try
        {
            onComplete?.Invoke(success);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error invoking post-create callback: " + ex.Message);
        }
    }
    public void OnPlayButtonClicked()
    {
        StartCoroutine(SendExpandRequest(success =>
        {
            if (success)
                Debug.Log("Expanded successfully");
            else
                Debug.LogWarning("Failed to expand");
        }));
    }

    private IEnumerator SendExpandRequest(Action<bool> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        Application.ExternalCall("expandFromUnity");
        onComplete?.Invoke(true);
    }
    catch (Exception ex)
    {
        Debug.LogError("Expand failed: " + ex.Message);
        onComplete?.Invoke(false);
    }
#else
        Debug.Log("Expand ignored (not WebGL)");
        onComplete?.Invoke(false);
#endif

        yield break;
    }

    public void OnCreateCustomPostButtonClicked()
    {
        StartCoroutine(SendCustomPostCreateRequest(success =>
        {
            if (success)
                Debug.Log("✅ Custom post created");
            else
                Debug.LogWarning("❌ Custom post creation failed");
        }));
    }
 
    private IEnumerator SendCustomPostCreateRequest(Action<bool> onComplete)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // ✅ Build base URL from Devvit WebView
    string origin = Application.absoluteURL;
    Uri uri = new Uri(origin);
    string baseUrl = uri.Scheme + "://" + uri.Host;
    string apiUrl = baseUrl + "/api/custom-post-create";

    // ✅ Prepare JSON body (ONLY ONCE)
    string json = JsonUtility.ToJson(new CustomPostCreateRequest
    {
        subredditName = "word_it_game_dev"
    });

    UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");

    yield return request.SendWebRequest();

    bool success = request.result == UnityWebRequest.Result.Success;

    if (!success)
    {
        Debug.LogWarning(
            "Error creating custom post: " +
            request.error +
            " | Response: " +
            request.downloadHandler.text
        );
    }
    else
    {
        Debug.Log("Custom post created successfully: " +
                  request.downloadHandler.text);

        CustomPostResponse response =
            JsonUtility.FromJson<CustomPostResponse>(
                request.downloadHandler.text
            );

        if (!string.IsNullOrEmpty(response.postUrl))
        {
            Debug.Log("Opening post: " + response.postUrl);
            OpenPostWithUrl(response.postUrl);
        }
        else
        {
            Debug.LogError("postUrl missing in response");
        }
    }

    onComplete?.Invoke(success);
#else
        Debug.LogWarning("Custom post creation only works in WebGL.");
        onComplete?.Invoke(false);
        yield break;
#endif
    }
    [Serializable]
    public class CustomPostCreateRequest
    {
        public string subredditName;
    }

    [Serializable]
    public class CustomPostResponse
    {
        public string status;
        public string postUrl;
    }

    [DllImport("__Internal")]
    private static extern void OpenPostWithUrl(string url);


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
           // SaveScore(10);

        }
        if (Input.GetKeyUp(KeyCode.G))
        {

           // StartCoroutine(AddScore());

        }
    }
    IEnumerator AddScore()
    {
        UnityWebRequest request = UnityWebRequest.Get("/api/init");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Error fetching init data: " + request.error + " — this will occur when running in Unity.");

            yield break;
        }

        // Parse and store the data
        InitResponse data = JsonUtility.FromJson<InitResponse>(request.downloadHandler.text);

        if (globalScoreText != null && !string.IsNullOrEmpty(data.globalscore))
            globalScore = int.Parse(data.globalscore);

        globalScore += 10;
        SaveGlobalScore(globalScore);

    }
    private const string PLAYER_NAME_KEY = "PLAYER_NAME";

   
   
   

    //Save Score


    [System.Serializable]
    private class SaveScorePayload
    {

        public string type;
        public string username;
        public string postId;
        public int score;
    }


    // Sends level completion data to server. Accepts an optional callback invoked with true on success, false on failure.
    public void SaveScore(int score)
    {
        StartCoroutine(SaveScoree(score));
    }
    IEnumerator SaveScoree(int finalScore)
    {
        SaveScorePayload payload = new SaveScorePayload
        {
            score = finalScore
        };

        string json = JsonUtility.ToJson(payload);

        UnityWebRequest request =
            new UnityWebRequest("/api/save-score", "POST");

        request.uploadHandler =
            new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Save score failed");
            Debug.LogError("Status: " + request.responseCode);
            Debug.LogError("Body: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Score saved successfully: " + finalScore);
        }
    }

    //GlobalScore Data

    [System.Serializable]
    private class GlobalScoreData
    {

        public string type;
        public string username;
        public string postId;
        public string globalscore;
    }


    // Sends level completion data to server. Accepts an optional callback invoked with true on success, false on failure.
    public void SaveGlobalScore(int score)
    {
        StartCoroutine(SendGlobalScoreRequest(score));
    }
    private IEnumerator SendGlobalScoreRequest(int score)
    {
        // Prepare request
        UnityWebRequest request = new UnityWebRequest("/api/save-global-score", "POST");

        GlobalScoreData data = new()
        {
            type = "save-global-score",
            username = currentUsername,
            postId = currentPostId,
            globalscore = score.ToString(),
        };

        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;

        if (!success)
        {
            Debug.LogWarning("Error sending score data: " + request.error + " — this will occur when running in Unity.");
        }
        else
        {
            Debug.Log("GlobalScore is saved: " + data.globalscore);
            globalScoreText.text = "GlobalScore:  " + data.globalscore;
        }
    }


}