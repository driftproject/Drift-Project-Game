using UnityEngine;

public class LightCar : MonoBehaviour
{
    public GameObject Lights;
    bool Light;

    private void Start()
    {
        Light = false;
        Lights.SetActive(Light);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Light = !Light;
            Lights.SetActive(Light);
        }
    }
}
