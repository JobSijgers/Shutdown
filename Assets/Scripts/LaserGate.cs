using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGate : MonoBehaviour
{
    [SerializeField] private BoxCollider barrier;
    [SerializeField] private Transform laserParent;
    private List<ParticleSystem> lasers = new();
    private bool gotKeys = false;
    private bool gotMaze = false;


    void Start()
    {
        for (int i = 0; i < laserParent.childCount; i++)
        {
            lasers.Add(laserParent.GetChild(i).GetComponent<ParticleSystem>());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        { StartCoroutine(DisableGate()); }
    }

    private IEnumerator DisableGate()
    {

        foreach (ParticleSystem laser in lasers)
        {
            var emission = laser.emission;
            emission.rateOverTime = 0;
            var subParticles = laser.transform.GetChild(0).GetComponent<ParticleSystem>().emission;
            subParticles.rateOverTime = 0;
        }

        yield return new WaitForSeconds(3);
        Destroy(barrier);

        yield return new WaitForSeconds(1); // to make sure everything is finished
        Destroy(gameObject);
    }


    public void CompleteKeys()
    {
        gotKeys = true;
        if (gotMaze)
        {
            StartCoroutine(DisableGate());
        }
    }

    public void CompleteMaze()
    {
        gotMaze = true;
        if (gotKeys)
        {
            StartCoroutine(DisableGate());
        }
    }


}
