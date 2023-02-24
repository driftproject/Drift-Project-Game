using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Map
{
    public Vector3 pos;
    public GameObject prefab;
    public int checkpointsCount;
}

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static CarController PlayerCar { get { return Instance.m_PlayerCar; } }
    public static bool RaceIsStarted;
    public static bool RaceIsEnded;

    public float MinSlipForScore = 0.6f;
    public float MaxTimeForDrift = 2;
    public float TimeFromDriftLastX = 0;
    public int DriftScoreX = 1;
    public float DriftScore = 0;
    public float CurrentDriftScore = 0;

    [HideInInspector] public int nextCheckpointIndex = 0;

    public GameObject[] CarPrefabs;
    public Map[] MapPrefabs;

    public CarController m_PlayerCar;

    float TimeLeftForDrift;
    int checkpointsCount;

    [Header("End of race")]
    public GameObject EndracePanel;
    public Text EndraceTimeText;

    public void MenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void CreateCar()
    {
        Instantiate(CarPrefabs[PlayerPrefs.GetInt("CurrentCar")], Vector3.zero, Quaternion.identity);
    }

    void CreateMap()
    {
        Instantiate(MapPrefabs[PlayerPrefs.GetInt("CurrentMap")].prefab, MapPrefabs[PlayerPrefs.GetInt("CurrentMap")].pos, Quaternion.identity);
    }

    public void RaceCheckpoint()
    {
        nextCheckpointIndex++;

        if (checkpointsCount == nextCheckpointIndex)
        {
            EndRace();
        }
    }

    void EndRace()
    {
        Pause.pause = true;
        RaceIsEnded = true;
        EndracePanel.SetActive(true);

        TimeManager tm = TimeManager.instance;
        EndraceTimeText.text = string.Format("{0:00}.{1:00}.{2}", Mathf.Floor(tm.timer / 60), Mathf.Floor(tm.timer % 60), Mathf.Floor(tm.timer % 1 * 100));
        GetComponent<NetworkController>().PostCreateRatingRequest(GetComponent<NetworkController>().URL + "/rating/create", (int)TimeManager.instance.timer, (int)(DriftScore + (CurrentDriftScore * DriftScoreX)));
    }

    void Awake()
    {
        Instance = this;
        CreateMap();
        CreateCar();
        m_PlayerCar = FindObjectOfType<CarController>();
        DriftScore = 0;

        m_PlayerCar.GetComponent<UserControl>().enabled = true;
        m_PlayerCar.GetComponent<AudioListener>().enabled = true;

        nextCheckpointIndex = 0;

        RaceIsStarted = true;
    }

    private void Start()
    {
        EndracePanel.SetActive(false);
        checkpointsCount = MapPrefabs[PlayerPrefs.GetInt("CurrentMap")].checkpointsCount;
    }

    private void Update()
    {
        if (!Pause.pause)
        {
            if (CarController.instance.CurrentMaxSlip >= MinSlipForScore && CarController.instance.SpeedInHour > 5)
            {
                CurrentDriftScore += 3f;
                TimeFromDriftLastX += Time.deltaTime;
                TimeLeftForDrift = MaxTimeForDrift;

                if (TimeFromDriftLastX > 8 && DriftScoreX < 5)
                {
                    DriftScoreX++;
                    TimeFromDriftLastX = 0;
                }
            }
            else
            {
                TimeLeftForDrift -= Time.deltaTime;
            }

            if (TimeLeftForDrift <= 0)
            {
                DriftScore += CurrentDriftScore * DriftScoreX;
                TimeFromDriftLastX = 0;
                CurrentDriftScore = 0;
                DriftScoreX = 1;
            }
        }
    }
}
