using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScore : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;

    public void Awake()
    {
        entryContainer = transform.Find("hsContainerTable");
        entryTemplate = entryContainer.Find("hsEntryTemplate");

        entryTemplate.gameObject.SetActive(false);

        float templateHeight = 30f;

        for (int i=0; i<10; i++)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);
            entryTransform.gameObject.SetActive(true);

            /*int rank = i + 1;
            string rankString;
            switch (rank)
            {
                default:
                    rankString = rank + "TH"; break;

                case 1: rankString = "1st"; break;
                case 2: rankString = "2nd"; break;
                case 3: rankString = "3rd"; break;
            }
            entryTransform.Find("posText").GetComponent<Text>().text = rankString;*/
        }
    }
}
