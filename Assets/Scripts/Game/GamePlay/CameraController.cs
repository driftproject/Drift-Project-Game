using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public static CameraController instance;
    public Transform CameraHolder;
    public float SetPositionSpeed = 1;
    public float VelocityMultiplier;

    public float MinDistanceForRotation = 0.1f;
    public float SetRotationSpeed = 1;

    CarController targetCar { get { return GameController.PlayerCar; } }

    float SqrMinDistance;

    Vector3 targetPoint
    {
        get
        {
            if (targetCar == null)
            {
                return transform.position;
            }
            Vector3 result = targetCar.RB.velocity * VelocityMultiplier;
            result += targetCar.transform.position;
            result.y = targetCar.transform.position.y;
            return result;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        transform.position = targetPoint;
    }

    protected override void AwakeSingleton()
    {
        UpdateActiveCamera();
    }

    public void UpdateActiveCamera()
    {
        CameraHolder.gameObject.SetActive(true);

        SqrMinDistance = MinDistanceForRotation * 2;

        if ((targetPoint - transform.position).sqrMagnitude >= SqrMinDistance)
        {
            Quaternion rotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);
            CameraHolder.rotation = rotation;
        }
    }

    void Update()
    {
        if ((targetPoint - transform.position).sqrMagnitude >= SqrMinDistance)
        {
            Quaternion rotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);
            CameraHolder.rotation = Quaternion.Lerp(CameraHolder.rotation, rotation, Time.deltaTime * SetRotationSpeed);
        }

        transform.position = Vector3.LerpUnclamped(transform.position, targetPoint, Time.deltaTime * SetPositionSpeed);
    }
}
