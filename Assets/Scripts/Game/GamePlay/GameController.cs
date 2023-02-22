using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class Map
{
    public Vector3 pos;
    public GameObject prefab;
}

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static CarController PlayerCar { get { return Instance.m_PlayerCar; } }
    public static bool RaceIsStarted { get { return true; } }
    public static bool RaceIsEnded { get { return false; } }

    public float MinSlipForScore = 0.6f;
    public float MaxTimeForDrift = 2;
    public float TimeFromDriftLastX = 0;
    public int DriftScoreX = 1;
    public float DriftScore = 0;
    public float CurrentDriftScore = 0;

    public GameObject[] CarPrefabs;
    public Map[] MapPrefabs;

    float TimeLeftForDrift;

    CarController m_PlayerCar;

    void CreateCar()
    {
        Instantiate(CarPrefabs[PlayerPrefs.GetInt("CurrentCar")], Vector3.zero, Quaternion.identity);
    }

    void CreateMap()
    {
        Instantiate(MapPrefabs[PlayerPrefs.GetInt("CurrentMap")].prefab, MapPrefabs[PlayerPrefs.GetInt("CurrentMap")].pos, Quaternion.identity);
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

            if (Input.GetKeyDown(KeyCode.Return))
            {
                m_PlayerCar.transform.position = new Vector3(0, 0, 0);
                m_PlayerCar.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }
}
