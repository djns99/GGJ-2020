using System.Collections;
using System.Collections.Generic;
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
        if (_instance != null)
            Destroy(_instance);
        else
            _instance = this;
        DontDestroyOnLoad(this);
    }

    public void AssignPlayer()
    {
        //player_name = 
        GameObject inputField = GameObject.Find("InputField");
        if ( inputField != null)
        {
            player_name = inputField.transform.Find("Text").GetComponent<Text>().text;
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
