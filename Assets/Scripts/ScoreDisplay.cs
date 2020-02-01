using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log(GetComponent<TextMeshProUGUI>());
            Debug.Log(GameManager.Instance.score.ToString());
            GetComponent<TextMeshProUGUI>().text = GameManager.Instance.score.ToString();
        }
    }
}
