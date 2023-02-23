using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    GameController gc;
    public Text mainTimeText;

    private void Update()
    {
        float mainTime = Time.time;

        mainTimeText.text = $"{Mathf.Floor(mainTime / 60)}.{Mathf.Floor(mainTime % 60)}";
    }
}
