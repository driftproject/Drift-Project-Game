using UnityEngine.SceneManagement;
using UnityEngine;

public class ChooseMap : MonoBehaviour
{
    public GameObject[] maps;
    public int index;

    private void Start()
    {
        index = -1;
        NextMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextMap();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            PrevMap();
        else if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene("Game");
    }

    public void NextMap()
    {
        index++;
        if (index >= maps.Length)
            index = 0;

        for (int i = 0; i < maps.Length; i++)
        {
            if (i == index)
                maps[i].SetActive(true);
            else
                maps[i].SetActive(false);
        }
        PlayerPrefs.SetInt("CurrentMap", index);
    }

    public void PrevMap()
    {
        index--;
        if (index <= -1)
            index = maps.Length - 1;

        for (int i = 0; i < maps.Length; i++)
        {
            if (i == index)
                maps[i].SetActive(true);
            else
                maps[i].SetActive(false);
        }
        PlayerPrefs.SetInt("CurrentMap", index);
    }

    public void QuitFromMenu()
    {
        SceneManager.LoadScene("Start Menu");
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
}
