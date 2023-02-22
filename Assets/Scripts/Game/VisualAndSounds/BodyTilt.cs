using UnityEngine;

[RequireComponent(typeof(CarController))]
public class BodyTilt : MonoBehaviour
{
    [SerializeField] Transform Body;
    [SerializeField] float MaxAngle = 10;
    [SerializeField] float AngleVelocityMultiplayer = 0.2f;
    [SerializeField] float RearAngleVelocityMultiplayer = 0.4f;
    [SerializeField] float MaxTiltOnSpeed = 60;

    CarController Car;
    float Angle;

    private void Awake()
    {
        Car = GetComponent<CarController>();
    }

    private void Update()
    {
        if (Car.CarDirection == 1)
            Angle = -Car.VelocityAngle * AngleVelocityMultiplayer;
        else if (Car.CarDirection == -1)
        {
            Angle = MathExtentions.LoopClamp(Car.VelocityAngle + 180, -180, 180) * RearAngleVelocityMultiplayer;
        }
        else
        {
            Angle = 0;
        }

        Angle *= Mathf.Clamp01(Car.SpeedInHour / MaxTiltOnSpeed);
        Angle = Mathf.Clamp(Angle, -MaxAngle, MaxAngle);
        Body.localRotation = Quaternion.AngleAxis(Angle, Vector3.forward);
    }
}
