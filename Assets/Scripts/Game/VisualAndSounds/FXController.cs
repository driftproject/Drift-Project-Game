using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    public static FXController Instance;
    public ParticleSystem AsphaltSmokeParticles;
    public TrailRenderer TrailRef;
    public Transform TrailHolder;

    private void Awake()
    {
        Instance = this;
    }

    Queue<TrailRenderer> FreeTrails = new Queue<TrailRenderer>();

    public TrailRenderer GetTrail(Vector3 startPos)
    {
        TrailRenderer trail;
        if (FreeTrails.Count > 0)
        {
            trail = FreeTrails.Dequeue();
        }
        else
        {
            trail = Instantiate(TrailRef, TrailHolder);
        }

        trail.transform.position = startPos;
        trail.gameObject.SetActive(true);

        return trail;
    }

    public void SetFreeTrail(TrailRenderer trail)
    {
        StartCoroutine(WaitVisibleTrail(trail));
    }

    private IEnumerator WaitVisibleTrail(TrailRenderer trail)
    {
        trail.transform.SetParent(TrailHolder);
        yield return new WaitForSeconds(trail.time);
        trail.Clear();
        trail.gameObject.SetActive(false);
        FreeTrails.Enqueue(trail);
    }
}
