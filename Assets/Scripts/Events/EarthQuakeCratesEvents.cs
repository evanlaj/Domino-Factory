using UnityEngine;

public class EarthQuakeCratesEvents : MonoBehaviour, IEvent
{
    double eventDuration;
    double spawnBlocksByDuration;
    GameObject[] crates;
    float duration = 0.8f;

    public void StartEvent()
    {
        eventDuration = 4;
        spawnBlocksByDuration = 2;

        crates = GameObject.FindGameObjectsWithTag("Crate");

        InvokeRepeating(nameof(InstantiateBlocks), duration, duration);
    }


    void InstantiateBlocks()
    {
        CameraShake.Instance.Shake(0.025f, 0.4f);

        eventDuration -= duration;

        for (int i = 0; i < spawnBlocksByDuration; i++)
        {
            crates[Random.Range(0, crates.Length)].GetComponent<CrateBehaviour>().ReleaseDomino();
        }

        if (eventDuration < 1)
        {
            CancelInvoke(nameof(InstantiateBlocks));
        }
    }

}
