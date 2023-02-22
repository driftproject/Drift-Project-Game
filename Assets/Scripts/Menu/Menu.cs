using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject loginPanel;
    public InputField usernameField;
    public InputField passwordField;

    private void Start()
    {
        loginPanel.SetActive(false);
    }

    public void PlayGuestButton()
    {
        SceneManager.LoadScene("ChooseCar");
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void BackToMainMenuButton()
    {
        loginPanel.SetActive(false);
    }

    public void LogInPanelButton()
    {
        loginPanel.SetActive(true);
    }

    public void PlayButton()
    {
        NetworkController network_controller = FindObjectOfType<NetworkController>();
        network_controller.PostRequest("https://8499-176-62-183-85.eu.ngrok.io/player/login", usernameField.text, passwordField.text);
        //network_controller.GetRequest("https://8499-176-62-183-85.eu.ngrok.io/rating/all");
    }
}
