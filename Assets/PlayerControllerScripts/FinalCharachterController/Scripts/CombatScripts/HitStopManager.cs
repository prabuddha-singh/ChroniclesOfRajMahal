using UnityEngine;
using System.Collections;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class HitStopManager : MonoBehaviour{
     
     private static HitStopManager _instance;
     public static HitStopManager instance
        {
            get
            {
                if( _instance == null)
                {
                    GameObject go = new GameObject("HitStopManager");
                    _instance = go.AddComponent<HitStopManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    private Coroutine hitStopRoutine;
    
    public void DoHitStop(float Duration)
    {
        if( hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            Time.timeScale = 1f; 
        } 

        hitStopRoutine = StartCoroutine(HitStopCoroutine(Duration));
    }
     
    private IEnumerator HitStopCoroutine(float Duration)
    {
        Time.timeScale = 0f;

        float elapsedTime = 0f; 

        while(elapsedTime < Duration)
        {
          elapsedTime += Time.unscaledDeltaTime;
          yield return null;      
        }
        
        Time.timeScale = 1f;
        hitStopRoutine = null;
    } 
}
}

