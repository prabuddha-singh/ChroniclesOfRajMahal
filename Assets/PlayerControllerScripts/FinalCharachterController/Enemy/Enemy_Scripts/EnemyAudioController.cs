using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class EnemyAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        
        [Header("Swing SFX")]
        [SerializeField] private AudioClip SwingClip;

        [Header("Impact SFX")]
        [SerializeField] private AudioClip ImpactClip;

        public void PlaySwingSFX()
        {
            audioSource.PlayOneShot(SwingClip);
        }

        public void PlayImpactSFX()
        {
            audioSource.PlayOneShot(ImpactClip);
        }
    }
}

