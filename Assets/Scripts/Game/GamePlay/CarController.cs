using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum DriveType
{
    AWD,
    FWD,
    RWD
}

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [SerializeField] Wheel FrontLeftWheel;
    [SerializeField] Wheel FrontRightWheel;
    [SerializeField] Wheel RearLeftWheel;
    [SerializeField] Wheel RearRightWheel;
    [SerializeField] Transform COM;
    [SerializeField] List<ParticleSystem> BackFireParticles = new List<ParticleSystem>();

    [SerializeField] CarConfig CarConfig;

    #region Properties of car parameters

    float MaxMotorTorque;
    float MaxSteerAngle { get { return CarConfig.MaxSteerAngle; } }
    DriveType DriveType { get { return CarConfig.DriveType; } }
    bool AutomaticGearBox { get { return CarConfig.AutomaticGearBox; } }
    AnimationCurve MotorTorqueFromRpmCurve { get { return CarConfig.MotorTorqueFromRpmCurve; } }
    float MaxRPM { get { return CarConfig.MaxRPM; } }
    float MinRPM { get { return CarConfig.MinRPM; } }
    float CutOffRPM { get { return CarConfig.CutOffRPM; } }
    float CutOffOffsetRPM { get { return CarConfig.CutOffOffsetRPM; } }
    float RpmToNextGear { get { return CarConfig.RpmToNextGear; } }
    float RpmToPrevGear { get { return CarConfig.RpmToPrevGear; } }
    float MaxForwardSlipToBlockChangeGear { get { return CarConfig.MaxForwardSlipToBlockChangeGear; } }
    float RpmEngineToRpmWheelsLerpSpeed { get { return CarConfig.RpmEngineToRpmWheelsLerpSpeed; } }
    float[] GearsRatio { get { return CarConfig.GearsRatio; } }
    float MainRatio { get { return CarConfig.MainRatio; } }
    float ReversGearRatio { get { return CarConfig.ReversGearRatio; } }
    float MaxBrakeTorque { get { return CarConfig.MaxBrakeTorque; } }


    #endregion

    #region Properties of drif Settings

    bool EnableSteerAngleMultiplier { get { return CarConfig.EnableSteerAngleMultiplier; } }
    float MinSteerAngleMultiplier { get { return CarConfig.MinSteerAngleMultiplier; } }
    float MaxSteerAngleMultiplier { get { return CarConfig.MaxSteerAngleMultiplier; } }
    float MaxSpeedForMinAngleMultiplier { get { return CarConfig.MaxSpeedForMinAngleMultiplier; } }
    float SteerAngleChangeSpeed { get { return CarConfig.SteerAngleChangeSpeed; } }
    float MinSpeedForSteerHelp { get { return CarConfig.MinSpeedForSteerHelp; } }
    float HelpSteerPower { get { return CarConfig.HelpSteerPower; } }
    float OppositeAngularVelocityHelpPower { get { return CarConfig.OppositeAngularVelocityHelpPower; } }
    float PositiveAngularVelocityHelpPower { get { return CarConfig.PositiveAngularVelocityHelpPower; } }
    float MaxAngularVelocityHelpAngle { get { return CarConfig.MaxAngularVelocityHelpAngle; } }
    float AngularVelucityInMaxAngle { get { return CarConfig.AngularVelocityInMaxAngle; } }
    float AngularVelucityInMinAngle { get { return CarConfig.AngularVelocityInMinAngle; } }

    #endregion

    public CarConfig GetCarConfig { get { return CarConfig; } }
    public Wheel[] Wheels { get; private set; }
    public System.Action BackFireAction;

    float[] AllGearsRatio;

    Rigidbody _RB;
    public Rigidbody RB
    {
        get
        {
            if (!_RB)
            {
                _RB = GetComponent<Rigidbody>();
            }
            return _RB;
        }
    }

    [HideInInspector] public float CurrentMaxSlip;
    public int CurrentMaxSlipWheelIndex { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float SpeedInHour { get { return CurrentSpeed * 2.4f; } }
    public int CarDirection { get { return CurrentSpeed < 1 ? 0 : (VelocityAngle < 90 && VelocityAngle > -90 ? 1 : -1); } }

    public static CarController instance;

    float CurrentSteerAngle;
    float CurrentAcceleration;
    float CurrentBrake;
    bool InHandBrake;

    int FirstDriveWheel;
    int LastDriveWheel;

    private void Awake()
    {
        instance = this;
        RB.centerOfMass = COM.localPosition;

        Wheels = new Wheel[4] {
            FrontLeftWheel,
            FrontRightWheel,
            RearLeftWheel,
            RearRightWheel
        };

        switch (DriveType)
        {
            case DriveType.AWD:
                FirstDriveWheel = 0;
                LastDriveWheel = 3;
                break;
            case DriveType.FWD:
                FirstDriveWheel = 0;
                LastDriveWheel = 1;
                break;
            case DriveType.RWD:
                FirstDriveWheel = 2;
                LastDriveWheel = 3;
                break;
        }

        MaxMotorTorque = CarConfig.MaxMotorTorque / (LastDriveWheel - FirstDriveWheel + 1);

        AllGearsRatio = new float[GearsRatio.Length + 2];
        AllGearsRatio[0] = ReversGearRatio * MainRatio;
        AllGearsRatio[1] = 0;
        for (int i = 0; i < GearsRatio.Length; i++)
        {
            AllGearsRatio[i + 2] = GearsRatio[i] * MainRatio;
        }

        foreach (var particles in BackFireParticles)
        {
            BackFireAction += () => particles.Emit(2);
        }
    }

    public void UpdateControls(float horizontal, float vertical, bool handBrake, bool nitro)
    {
        float targetSteerAngle = horizontal * MaxSteerAngle;

        if (nitro == false)
        {
            CurrentAcceleration = vertical;
        }
        else
        {
            CurrentAcceleration = vertical * 3;
            targetSteerAngle = horizontal * MaxSteerAngle / 4;
            PlayBackfireWithProbability();
        }

        if (EnableSteerAngleMultiplier)
        {
            targetSteerAngle *= Mathf.Clamp(1 - SpeedInHour / MaxSpeedForMinAngleMultiplier, MinSteerAngleMultiplier, MaxSteerAngleMultiplier);
        }

        CurrentSteerAngle = Mathf.MoveTowards(CurrentSteerAngle, targetSteerAngle, Time.deltaTime * SteerAngleChangeSpeed);

        InHandBrake = handBrake;
    }

    private void Update()
    {
        for (int i = 0; i < Wheels.Length; i++)
        {
            Wheels[i].UpdateVisual();
        }
    }

    private void FixedUpdate()
    {
        CurrentSpeed = RB.velocity.magnitude;

        UpdateSteerAngleLogic();
        UpdateRpmAndTorqueLogic();

        CurrentMaxSlip = Wheels[0].CurrentMaxSlip;
        CurrentMaxSlipWheelIndex = 0;

        if (InHandBrake)
        {
            RearLeftWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
            RearRightWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
            FrontLeftWheel.WheelCollider.brakeTorque = 0;
            FrontRightWheel.WheelCollider.brakeTorque = 0;
        }

        for (int i = 0; i < Wheels.Length; i++)
        {
            if (!InHandBrake)
            {
                Wheels[i].WheelCollider.brakeTorque = CurrentBrake;
            }

            Wheels[i].FixedUpdate();

            if (CurrentMaxSlip < Wheels[i].CurrentMaxSlip)
            {
                CurrentMaxSlip = Wheels[i].CurrentMaxSlip;
                CurrentMaxSlipWheelIndex = i;
            }
        }

    }

    #region Steer help logic

    public float VelocityAngle { get; private set; }

    void UpdateSteerAngleLogic()
    {
        var needHelp = SpeedInHour > MinSpeedForSteerHelp && CarDirection > 0;
        float targetAngle = 0;
        VelocityAngle = -Vector3.SignedAngle(RB.velocity, transform.TransformDirection(Vector3.forward), Vector3.up);

        if (needHelp)
        {
            targetAngle = Mathf.Clamp(VelocityAngle * HelpSteerPower, -MaxSteerAngle, MaxSteerAngle);
        }

        targetAngle = Mathf.Clamp(targetAngle + CurrentSteerAngle, -(MaxSteerAngle + 10), MaxSteerAngle + 10);

        Wheels[0].WheelCollider.steerAngle = targetAngle;
        Wheels[1].WheelCollider.steerAngle = targetAngle;

        if (needHelp)
        {
            var absAngle = Mathf.Abs(VelocityAngle);

            float currentAngularProcent = absAngle / MaxAngularVelocityHelpAngle;

            var currAngle = RB.angularVelocity;

            if (VelocityAngle * CurrentSteerAngle > 0)
            {
                var angularVelocityMagnitudeHelp = OppositeAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                currAngle.y += angularVelocityMagnitudeHelp * currentAngularProcent;
            }
            else if (!Mathf.Approximately(CurrentSteerAngle, 0))
            {
                var angularVelocityMagnitudeHelp = PositiveAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                currAngle.y += angularVelocityMagnitudeHelp * (1 - currentAngularProcent);
            }

            var maxMagnitude = ((AngularVelucityInMaxAngle - AngularVelucityInMinAngle) * currentAngularProcent) + AngularVelucityInMinAngle;
            currAngle.y = Mathf.Clamp(currAngle.y, -maxMagnitude, maxMagnitude);
            RB.angularVelocity = currAngle;
        }
    }

    #endregion

    #region Rpm and torque logic

    public int CurrentGear { get; private set; }
    public int CurrentGearIndex { get { return CurrentGear + 1; } }
    public float EngineRPM { get; private set; }
    public float GetMaxRPM { get { return MaxRPM; } }
    public float GetMinRPM { get { return MinRPM; } }
    public float GetInCutOffRPM { get { return CutOffRPM - CutOffOffsetRPM; } }

    float CutOffTimer;
    bool InCutOff;

    void UpdateRpmAndTorqueLogic()
    {

        if (InCutOff)
        {
            if (CutOffTimer > 0)
            {
                CutOffTimer -= Time.fixedDeltaTime;
                EngineRPM = Mathf.Lerp(EngineRPM, GetInCutOffRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
            }
            else
            {
                InCutOff = false;
            }
        }

        if (!GameController.RaceIsStarted)
        {
            if (InCutOff)
                return;

            float rpm = CurrentAcceleration > 0 ? MaxRPM : MinRPM;
            float speed = CurrentAcceleration > 0 ? RpmEngineToRpmWheelsLerpSpeed : RpmEngineToRpmWheelsLerpSpeed * 0.2f;
            EngineRPM = Mathf.Lerp(EngineRPM, rpm, speed * Time.fixedDeltaTime);
            if (EngineRPM >= CutOffRPM)
            {
                PlayBackfire();
                InCutOff = true;
                CutOffTimer = CarConfig.CutOffTime;
            }
            return;
        }

        float minRPM = 0;
        for (int i = FirstDriveWheel + 1; i <= LastDriveWheel; i++)
        {
            minRPM += Wheels[i].WheelCollider.rpm;
        }

        minRPM /= LastDriveWheel - FirstDriveWheel + 1;

        if (!InCutOff)
        {
            float targetRPM = ((minRPM + 20) * Mathf.Abs(AllGearsRatio[CurrentGearIndex]));
            targetRPM = Mathf.Clamp(targetRPM, MinRPM, MaxRPM);
            EngineRPM = Mathf.Lerp(EngineRPM, targetRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
        }

        if (EngineRPM >= CutOffRPM)
        {
            PlayBackfireWithProbability();
            InCutOff = true;
            CutOffTimer = CarConfig.CutOffTime;
            return;
        }

        if (!Mathf.Approximately(CurrentAcceleration, 0))
        {
            if (CarDirection * CurrentAcceleration >= 0)
            {
                CurrentBrake = 0;

                float motorTorqueFromRpm = MotorTorqueFromRpmCurve.Evaluate(EngineRPM * 0.001f);
                var motorTorque = CurrentAcceleration * (motorTorqueFromRpm * (MaxMotorTorque * AllGearsRatio[CurrentGearIndex]));
                if (Mathf.Abs(minRPM) * AllGearsRatio[CurrentGearIndex] > MaxRPM)
                {
                    motorTorque = 0;
                }

                float maxWheelRPM = AllGearsRatio[CurrentGearIndex] * EngineRPM;
                for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
                {
                    if (Wheels[i].WheelCollider.rpm <= maxWheelRPM)
                    {
                        Wheels[i].WheelCollider.motorTorque = motorTorque;
                    }
                    else
                    {
                        Wheels[i].WheelCollider.motorTorque = 0;
                    }
                }
            }
            else
            {
                CurrentBrake = MaxBrakeTorque;
            }
        }
        else
        {
            CurrentBrake = 0;

            for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
            {
                Wheels[i].WheelCollider.motorTorque = 0;
            }
        }

        if (AutomaticGearBox)
        {

            bool forwardIsSlip = false;
            for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
            {
                if (Wheels[i].CurrentForwardSleep > MaxForwardSlipToBlockChangeGear)
                {
                    forwardIsSlip = true;
                    break;
                }
            }

            float prevRatio = 0;
            float newRatio = 0;

            if (!forwardIsSlip && EngineRPM > RpmToNextGear && CurrentGear >= 0 && CurrentGear < (AllGearsRatio.Length - 2))
            {
                prevRatio = AllGearsRatio[CurrentGearIndex];
                CurrentGear++;
                newRatio = AllGearsRatio[CurrentGearIndex];
            }
            else if (EngineRPM < RpmToPrevGear && CurrentGear > 0 && (EngineRPM <= MinRPM || CurrentGear != 1))
            {
                prevRatio = AllGearsRatio[CurrentGearIndex];
                CurrentGear--;
                newRatio = AllGearsRatio[CurrentGearIndex];
            }

            if (!Mathf.Approximately(prevRatio, 0) && !Mathf.Approximately(newRatio, 0))
            {
                EngineRPM = Mathf.Lerp(EngineRPM, EngineRPM * (newRatio / prevRatio), RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime); //EngineRPM * (prevRatio / newRatio);// 
            }

            if (CarDirection <= 0 && CurrentAcceleration < 0)
            {
                CurrentGear = -1;
            }
            else if (CurrentGear <= 0 && CarDirection >= 0 && CurrentAcceleration > 0)
            {
                CurrentGear = 1;
            }
            else if (CarDirection == 0 && CurrentAcceleration == 0)
            {
                CurrentGear = 0;
            }
        }
    }

    void PlayBackfireWithProbability()
    {
        PlayBackfireWithProbability(GetCarConfig.ProbabilityBackfire);
    }

    void PlayBackfire()
    {
        BackFireAction.SafeInvoke();
    }

    void PlayBackfireWithProbability(float probability)
    {
        if (Random.Range(0f, 1f) <= probability)
        {
            BackFireAction.SafeInvoke();
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        var centerPos = transform.position;
        var velocity = transform.position + (Vector3.ClampMagnitude(RB.velocity, 4));
        var forwardPos = transform.TransformPoint(Vector3.forward * 4);

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(centerPos, 0.2f);
        Gizmos.DrawLine(centerPos, velocity);
        Gizmos.DrawLine(centerPos, forwardPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(forwardPos, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(velocity, 0.2f);

        Gizmos.color = Color.white;
    }

}

[System.Serializable]
public class CarConfig
{
    [Header("Steer Settings")]
    public float MaxSteerAngle = 25;

    [Header("Engine and power settings")]
    public DriveType DriveType = DriveType.RWD;
    public bool AutomaticGearBox = true;
    public float MaxMotorTorque = 150;
    public AnimationCurve MotorTorqueFromRpmCurve;
    public float MaxRPM = 7000;
    public float MinRPM = 700;
    public float CutOffRPM = 6800;
    public float CutOffOffsetRPM = 500;
    public float CutOffTime = 0.1f;
    [Range(0, 1)] public float ProbabilityBackfire = 0.2f;
    public float RpmToNextGear = 6500;
    public float RpmToPrevGear = 4500;
    public float MaxForwardSlipToBlockChangeGear = 0.5f;
    public float RpmEngineToRpmWheelsLerpSpeed = 15;
    public float[] GearsRatio;
    public float MainRatio;
    public float ReversGearRatio;

    [Header("Braking settings")]
    public float MaxBrakeTorque = 1000;

    [Header("Helper settings")]

    public bool EnableSteerAngleMultiplier = true;
    public float MinSteerAngleMultiplier = 0.05f;
    public float MaxSteerAngleMultiplier = 1f;
    public float MaxSpeedForMinAngleMultiplier = 250;
    [Space(10)]

    public float SteerAngleChangeSpeed;
    public float MinSpeedForSteerHelp;
    [Range(0f, 1f)] public float HelpSteerPower;
    public float OppositeAngularVelocityHelpPower = 0.1f;
    public float PositiveAngularVelocityHelpPower = 0.1f;
    public float MaxAngularVelocityHelpAngle;
    public float AngularVelocityInMaxAngle;
    public float AngularVelocityInMinAngle;
}
