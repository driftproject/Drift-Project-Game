using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text SpeedText;
    public Text DriftText;
    public Text CurrentDriftText;
    public Text CurrentDriftXText;
    CarController Car;

    private void Start()
    {
        Car = CarController.instance;
    }

    void Update()
    {
        SpeedText.text = Mathf.RoundToInt(Car.SpeedInHour).ToString();
        DriftText.text = Mathf.RoundToInt(GameController.Instance.DriftScore).ToString();
        CurrentDriftText.text = Mathf.RoundToInt(GameController.Instance.CurrentDriftScore).ToString();
        if (GameController.Instance.DriftScoreX > 1)
            CurrentDriftXText.text = GameController.Instance.DriftScoreX.ToString() + "x";
        else
            CurrentDriftXText.text = "";
    }
}
