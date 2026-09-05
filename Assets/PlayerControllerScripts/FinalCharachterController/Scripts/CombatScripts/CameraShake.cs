using Cinemachine;
using UnityEngine;
using System.Collections;


namespace PrabuddhaSingh.FinalCharachterController
{
   public class CameraShake : MonoBehaviour{

    public static CameraShake Instance;

    private Cinemachine.CinemachineVirtualCamera vcam;
    private Cinemachine.CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        vcam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();   
    }

    public void Shake(float duration , float strength)
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(ShakeCoroutine(duration,strength));    
        }    

        StartCoroutine(ShakeCoroutine(duration,strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float elapsed = 0f;
        noise.m_AmplitudeGain = strength;
        noise.m_FrequencyGain = 2f;
        while(elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        noise.m_AmplitudeGain = 0f;
        shakeRoutine = null;
    }
    
  } 
}


