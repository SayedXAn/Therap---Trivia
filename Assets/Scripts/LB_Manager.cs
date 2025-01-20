using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using System;

public class LBManager : MonoBehaviour
{
    public GameManager mngr;
    public GameObject LBUI;
    public GameObject loadingScreen;
    public GameObject mainLB;
    public int leaderboardSize = 15;
    public float loadingTime = 2f;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject entryPrefab;
    //public UIManager uiman;
    private void Start()
    {
        /*for (int i = 0; i < 16; i++) 
        {
            SetEntry(" ", -1);
        }*/
    }
    
    private const string API_URL = "https://leaderboard-backend.mern.singularitybd.net/api/v1/leaderboard?game=2";
    private const string API_TOKEN = "9b1de5f407f1463e7b2a921bbce364";

    public int playerPosition = -1;
    public void GenerateLeaderboard()
    {
        StartCoroutine(EktuTimeDen());
        StartCoroutine(FetchLeaderboard());
    }

    private IEnumerator FetchLeaderboard()
    {
        // Clear existing entries
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // Send API request to get leaderboard
        string url = "https://leaderboard-backend.mern.singularitybd.net/api/v1/leaderboard?game=2";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("x-token", API_TOKEN);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error fetching leaderboard: {request.error}");
            yield break;
        }

        // Parse response
        var jsonResponse = request.downloadHandler.text;
        var leaderboardData = JsonUtility.FromJson<LeaderboardResponse>(jsonResponse);

        if (leaderboardData == null || leaderboardData.data.Count == 0)
        {
            Debug.LogWarning("No data found for leaderboard");
            yield break;
        }

        // Sort and take top 15 entries
        var sortedEntries = leaderboardData.data.OrderByDescending(entry => entry.score).Take(leaderboardSize).ToList();

        int pos = 0;
        foreach (var entry in sortedEntries)
        {
            pos++;
            var entryUI = Instantiate(entryPrefab, scrollViewContent);
            var posText = entryUI.transform.GetChild(1).GetComponent<TMP_Text>();
            var nameText = entryUI.transform.GetChild(2).GetComponent<TMP_Text>();
            var scoreText = entryUI.transform.GetChild(3).GetComponent<TMP_Text>();

            if (nameText != null && scoreText != null)
            {
                posText.text = "";
                if (pos == 1)
                {
                    entryUI.transform.GetChild(4).gameObject.SetActive(true);
                }
                else if(pos == 2)
                {
                    entryUI.transform.GetChild(5).gameObject.SetActive(true);
                }
                else if (pos == 3)
                {
                    entryUI.transform.GetChild(6).gameObject.SetActive(true);
                }
                else
                {
                    posText.text = "#" + pos.ToString();
                }
                nameText.text = entry.name;
                scoreText.text = entry.score.ToString();
            }
        }
    }


    public void SetEntry(string playerName, int playerScore)
    {
        StartCoroutine(SubmitScore(playerName, playerScore));
    }

    [Serializable]
    public class PlayerScore
    {
        public string name;
        public int score;
        public int game;

        public PlayerScore(string playerName, int playerScore, int gID)
        {
            name = playerName;
            score = playerScore;
            game = gID;
        }
    }
    [Serializable]
    public class ApiResponse
    {
        public int resState;
        public bool success;
        public string message;
        public Data data;
    }

    [Serializable]
    public class Data
    {
        public int position;
        public string id;
        public string name;
        public int score;
        public int game;
    }

    public IEnumerator SubmitScore(string playerName, int playerScore)
    {
        string url = "https://leaderboard-backend.mern.singularitybd.net/api/v1/score";

        // Create a PlayerScore object
        PlayerScore postData = new PlayerScore(playerName, playerScore, 2/*Trivia game id = 2*/);

        // Serialize to JSON
        string jsonBody = JsonUtility.ToJson(postData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", API_TOKEN);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error submitting score: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log("Score submitted successfully.");
            Debug.Log($"Response: {request.downloadHandler.text}");
            try
            {
                // Deserialize the response JSON
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);

                // Access the position
                //playerPosition = response.data.position;
                mngr.SetPlayerPos(response.data.position);
                //Debug.Log("Plaayerr posss = " + playerPosition);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}");
            }
            //Response: {"resState":201,"success":true,"message":"player inserted successfully","data":{"position":1,"id":"6785f039698f33be978b6622","name":"response","score":370}}
        }
    }



    /*public void DeleteLeaderboard()
    {
        StartCoroutine(ClearLeaderboard());
    }
    public IEnumerator ClearLeaderboard()
    {
        UnityWebRequest request = UnityWebRequest.Delete(API_URL);
        request.SetRequestHeader("x-token", API_TOKEN);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error clearing leaderboard: {request.error}");
        }
        else
        {
            Debug.Log("Leaderboard cleared successfully.");
        }
    }*/

    

    IEnumerator EktuTimeDen()
    {
        LBUI.SetActive(true);
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingTime);
        loadingScreen.SetActive(false);
        mainLB.SetActive(true);
    }


}

[System.Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardEntry> data;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int score;
}
