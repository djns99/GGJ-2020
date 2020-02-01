using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScore : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<HighScoreEntry> highScoreEntryList;
    private List<Transform> highScoreEntryTransformList;

    public void Awake()
    {
        entryContainer = transform.Find("hsContainerTable");
        entryTemplate = entryContainer.Find("hsEntryTemplate");

        entryTemplate.gameObject.SetActive(false);
        highScoreEntryList = new List<HighScoreEntry>()
        {
            new HighScoreEntry { score = 20000,   name = "Shawn"},
            new HighScoreEntry { score = 450000,  name = "Manu"},
            new HighScoreEntry { score = 900000,  name = "Cameron"},
            new HighScoreEntry { score = 345,     name = "Daniel"},
            new HighScoreEntry { score = 80000,   name = "Nubs"},
            new HighScoreEntry { score = 90000,   name = "Aaron"},
        };

        // Sort the entry listt by score
        for (int i = 0; i < highScoreEntryList.Count; i++)
        {
            for (int j = i+1; j < highScoreEntryList.Count; j++)
            {
                if (highScoreEntryList[j].score > highScoreEntryList[i].score) {
                    HighScoreEntry tmp = highScoreEntryList[i];
                    highScoreEntryList[i] = highScoreEntryList[j];
                    highScoreEntryList[j] = tmp;
                }
            }
        }

        highScoreEntryTransformList = new List<Transform>();

        foreach (HighScoreEntry highScoreEntry in highScoreEntryList)
        {
            CreateHighScoreEntryTransform(highScoreEntry, entryContainer, highScoreEntryTransformList);
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

    /*
     * Represents a single high score entry
     * */
    private class HighScoreEntry
    {
        public int score;
        public string name;  
    }
}
