using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    private float delayBeforeLoading = 2f;
    private float timeElapsed;

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > delayBeforeLoading && (SceneManager.GetActiveScene().buildIndex) == 0) {
            SceneManager.LoadScene(1);
        }
    }

    public void PlayGame()
    {
        GameManager.Instance.AssignPlayer();
        SceneManager.LoadScene(2);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(1);
    }
    
    public void QuitGame()
    {
        Debug.Log("GetLost");
        Application.Quit();
    }
}
