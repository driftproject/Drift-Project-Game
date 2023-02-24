using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public static bool pause = false;
    public GameObject pausePanel;
    public GameObject otherPanel;

    private void Start()
    {
        pausePanel.SetActive(false);
        otherPanel.SetActive(true);
        pause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            pause = !pause;

        if (pause)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            otherPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            otherPanel.SetActive(true);
        }
    }

    public void Resume()
    {
        if (GameController.RaceIsStarted == true && GameController.RaceIsEnded == false)
            pause = false;
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
