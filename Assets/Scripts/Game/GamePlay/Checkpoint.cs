using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameController gc;
    public int index;
    Vector3 startScale;

    private void Start()
    {
        gc = GameController.Instance;
        startScale = transform.localScale;
        gameObject.transform.localScale = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (gc.nextCheckpointIndex == index)
            gameObject.transform.localScale = startScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (index != gc.nextCheckpointIndex)
            return;

        gc.RaceCheckpoint();
        gameObject.SetActive(false);
    }
}
