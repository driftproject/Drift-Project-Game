using UnityEngine.SceneManagement;
using UnityEngine;

public class ChooseCar : MonoBehaviour
{
    public GameObject[] cars;
    public int index;

    private void Start()
    {
        index = -1;
        NextCar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextCar();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            PrevCar();
        else if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene("ChooseMap");
    }

    public void NextCar()
    {
        index++;
        if (index >= cars.Length)
            index = 0;

        for (int i = 0; i < cars.Length; i++)
        {
            if (i == index)
                cars[i].SetActive(true);
            else
                cars[i].SetActive(false);
        }
        PlayerPrefs.SetInt("CurrentCar", index);
    }

    public void PrevCar()
    {
        index--;
        if (index <= -1)
            index = cars.Length-1;

        for (int i = 0; i < cars.Length; i++)
        {
            if (i == index)
                cars[i].SetActive(true);
            else
                cars[i].SetActive(false);
        }
        PlayerPrefs.SetInt("CurrentCar", index);
    }

    public void QuitFromMenu()
    {
        SceneManager.LoadScene("Start Menu");
    }

    public void Play()
    {
        SceneManager.LoadScene("ChooseMap");
    }
}
