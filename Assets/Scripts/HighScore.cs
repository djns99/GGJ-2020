using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HighScore : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;
    private int firstWakeUp = 0;
    private List<Transform> highScoreEntryTransformList;

    public void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            entryContainer = transform.Find("hsContainerTable");
            entryTemplate = entryContainer.Find("hsEntryTemplate");

            entryTemplate.gameObject.SetActive(false);

            //AddHighScoreEntry(10000, "Sid");

            string jsonString = PlayerPrefs.GetString("highScoreTable");
            HighScores highScores = JsonUtility.FromJson<HighScores>(jsonString);

            //Sort the entry list by score
            for (int i = 0; i < highScores.highScoreEntryList.Count; i++)
            {
                for (int j = i + 1; j < highScores.highScoreEntryList.Count; j++)
                {
                    if (highScores.highScoreEntryList[j].score > highScores.highScoreEntryList[i].score)
                    {
                        HighScoreEntry tmp = highScores.highScoreEntryList[i];
                        highScores.highScoreEntryList[i] = highScores.highScoreEntryList[j];
                        highScores.highScoreEntryList[j] = tmp;
                    }
                }
            }

            highScoreEntryTransformList = new List<Transform>();

            foreach (HighScoreEntry highScoreEntry in highScores.highScoreEntryList)
            {
                CreateHighScoreEntryTransform(highScoreEntry, entryContainer, highScoreEntryTransformList);
            }

        }

    }

    private void CreateHighScoreEntryTransform(HighScoreEntry highScoreEntry, Transform container, List<Transform> transformlist)
    {
        float templateHeight = 30f;

        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformlist.Count);
        entryTransform.gameObject.SetActive(true);

        int rank = transformlist.Count + 1;
        string rankString;
        switch (rank)
        {
            default:
                rankString = rank + "TH"; break;

            case 1: rankString = "1ST"; break;
            case 2: rankString = "2ND"; break;
            case 3: rankString = "3RD"; break;
        }
        entryTransform.Find("posText").GetComponent<Text>().text = rankString;

        int score = highScoreEntry.score;
        entryTransform.Find("scoreText").GetComponent<Text>().text = score.ToString();

        string name = highScoreEntry.name;
        entryTransform.Find("nameText").GetComponent<Text>().text = name;

        transformlist.Add(entryTransform);
    }

    public void SubmitEntryToJson()
    {
        Debug.Log("I should be here");
        Debug.Log(GameManager.Instance.score);
        Debug.Log(GameManager.Instance.player_name);
        AddHighScoreEntry(GameManager.Instance.score, name: GameManager.Instance.player_name);
    }

    private void AddHighScoreEntry(int score, string name)
    {
        //Create High Score Entry
        HighScoreEntry highScoreEntry = new HighScoreEntry ( score, name );

        //Load saved high scores
        HighScores highScores = new HighScores();
        string jsonString = PlayerPrefs.GetString("highScoreTable");
        if (jsonString != null && firstWakeUp != 0) {
            highScores = JsonUtility.FromJson<HighScores>(jsonString);
        }
        firstWakeUp++;

        //Add new entry to highscores
        highScores.highScoreEntryList.Add(highScoreEntry);

        //Save updated high scores
        string json = JsonUtility.ToJson(highScores);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    public class HighScores
    {
        public List<HighScoreEntry> highScoreEntryList;

        public HighScores()
        {
            highScoreEntryList = new List<HighScoreEntry>();
        }
    }
    /*
     * Represents a single high score entry
     * */
    [System.Serializable]
    public class HighScoreEntry
    {
        public int score;
        public string name;

        public HighScoreEntry()
        {
            score = 0;
            name = "Player";
        }

        public HighScoreEntry(int score, string name)
        {
            this.score = score;
            this.name = name;
        }
    }
}
