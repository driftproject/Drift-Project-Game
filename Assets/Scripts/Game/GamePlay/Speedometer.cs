using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public float minSpeedAngle;
    public float maxSpeedAngle;
    public int maxSpeed;

    public Text gearboxText;

    public GameObject arrow;

    private void Update()
    {
        float speed = CarController.instance.SpeedInHour;
        float arrowAngle = Mathf.Clamp(minSpeedAngle - (speed * ((minSpeedAngle - maxSpeedAngle) / maxSpeed)), 13, 255);
        arrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, arrowAngle);

        int gear = CarController.instance.CurrentGear;

        if (gear == -1)
            gearboxText.text = "R";
        else if (gear == 0)
            gearboxText.text = "N";
        else
            gearboxText.text = gear.ToString();
    }
}
