using UnityEngine;
using PG_Physics.Wheel;

[System.Serializable]
public struct Wheel
{
    public WheelCollider WheelCollider;
    public Transform WheelView;
    public float SlipForGenerateParticle;
    public Vector3 TrailOffset;

    public float CurrentMaxSlip { get { return Mathf.Max(CurrentForwardSleep, CurrentSidewaysSleep); } }
    public float CurrentForwardSleep { get; private set; }
    public float CurrentSidewaysSleep { get; private set; }
    public WheelHit GetHit { get { return Hit; } }

    WheelHit Hit;
    TrailRenderer Trail;

    PG_WheelCollider m_PGWC;
    public PG_WheelCollider PG_WheelCollider
    {
        get
        {
            m_PGWC = WheelCollider.GetComponent<PG_WheelCollider>();
            return m_PGWC;
        }
    }

    FXController FXController { get { return FXController.Instance; } }
    Vector3 HitPoint;

    public void FixedUpdate()
    {
        if (WheelCollider.GetGroundHit(out Hit))
        {
            var prevForwar = CurrentForwardSleep;
            var prevSide = CurrentSidewaysSleep;

            CurrentForwardSleep = (prevForwar + Mathf.Abs(Hit.forwardSlip)) / 2;
            CurrentSidewaysSleep = (prevSide + Mathf.Abs(Hit.sidewaysSlip)) / 2;
        }
        else
        {
            CurrentForwardSleep = 0;
            CurrentSidewaysSleep = 0;
        }
    }

    public void UpdateVisual()
    {
        UpdateTransform();

        if (WheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
        {
            var particles = FXController.AsphaltSmokeParticles;
            var point = WheelCollider.transform.position;
            point.y = Hit.point.y;
            particles.transform.position = point;
            particles.Emit(1);

            if (Trail == null)
            {
                HitPoint = WheelCollider.transform.position;
                HitPoint.y = Hit.point.y;
                Trail = FXController.GetTrail(HitPoint);
                Trail.transform.SetParent(WheelCollider.transform);
                Trail.transform.localPosition += TrailOffset;
            }
        }
        else if (Trail != null)
        {
            FXController.SetFreeTrail(Trail);
            Trail = null;
        }
    }

    public void UpdateTransform()
    {
        Vector3 pos;
        Quaternion quat;
        WheelCollider.GetWorldPose(out pos, out quat);
        WheelView.position = pos;
        WheelView.rotation = quat;
    }

    public void UpdateFrictionConfig(PG_WheelColliderConfig config)
    {
        PG_WheelCollider.UpdateConfig(config);
    }
}
