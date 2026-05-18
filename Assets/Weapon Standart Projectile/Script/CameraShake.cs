using System.Collections;
using UnityEngine;

namespace Biostart.CameraEffects
{
    public class CameraShake : MonoBehaviour
    {
   
        private Vector3 originalPosition;

        private void Start()
        {
           
            originalPosition = transform.localPosition;
        }

 
        public void ShakeCamera(float duration, float magnitude)
        {
            StartCoroutine(Shake(duration, magnitude));
        }

     
        public IEnumerator Shake(float duration, float magnitude)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                
                float currentMagnitude = Mathf.Lerp(magnitude, 0f, elapsedTime / duration);

              
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentMagnitude,
                    Random.Range(-1f, 1f) * currentMagnitude,
                    0f
                );
                
        
                transform.localPosition = originalPosition + randomOffset;

   
                elapsedTime += Time.deltaTime;

                yield return null;
            }

    
            transform.localPosition = originalPosition;
        }
    }
}
