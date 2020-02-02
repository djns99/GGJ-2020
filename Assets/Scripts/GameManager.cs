using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public string player_name;
    public int score;

    public static GameManager Instance { get => _instance; set => _instance = value; }

    void Awake()
    {
        switch (_instance)
        {
            case null:
                _instance = this;
                Debug.Log("Game Manager active");
                break;
            default:
                Destroy(_instance);
                break;
        }

        DontDestroyOnLoad(this);
    }

    public void AssignPlayer()
    {
        GameObject inputField = GameObject.Find("Text Area");
        if ( inputField != null)
        {
            player_name = inputField.transform.Find("Text").GetComponent<TextMeshProUGUI>().text;
        }
        if (player_name == "") player_name = "Player1";
    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "CarScene")
        {
            if (GameObject.Find("Car") != null)
                score += 1;
            else
            {
                Debug.Log(score);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
