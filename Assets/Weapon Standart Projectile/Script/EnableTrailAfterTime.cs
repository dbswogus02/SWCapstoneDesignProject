using System.Collections;
using UnityEngine;

namespace Biostart.Trail
{
public class EnableTrailAfterTime : MonoBehaviour
{
    public float delay = 2f; 
    private TrailRenderer trailRenderer;

    void Start()
    {
     
        trailRenderer = GetComponent<TrailRenderer>();
        

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }


        StartCoroutine(EnableTrailWithDelay());
    }

    IEnumerator EnableTrailWithDelay()
    {
     
        yield return new WaitForSeconds(delay);

      
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
    }
}
}