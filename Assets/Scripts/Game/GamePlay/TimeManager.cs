using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    GameController gc;
    public Text mainTimeText;
    [HideInInspector] public float timer;
    public List<float> checkpointStartTimes = new List<float>();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Pause.pause == false)
            timer += Time.deltaTime;
        mainTimeText.text = string.Format("{0:00}.{1:00}.{2}", Mathf.Floor(timer / 60), Mathf.Floor(timer % 60), Mathf.Floor(timer % 1 * 100));
    }
}
