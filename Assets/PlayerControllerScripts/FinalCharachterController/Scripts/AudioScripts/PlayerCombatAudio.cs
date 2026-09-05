using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerCombatAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource AudioSource;

        [Header("Punch SFX")]
        [SerializeField] private AudioClip punch;

        [Header("Impact SFX")]
        [SerializeField] private AudioClip impact1;
        [SerializeField] private AudioClip impact2;

        [Header("Swing SFX")]
        [SerializeField] private AudioClip swing;

        public void PlayPunchSFX()
        {
            AudioSource.PlayOneShot(punch);
        }

        public void PlayImpactSFX(int index)
        {
            AudioClip clip = index switch
            {
                1 => impact1,
                2 => impact1,
                3 => impact2,
                _ => null
            };

            if (clip != null)
            {
                AudioSource.PlayOneShot(clip);
            }
        }
        public void PlaySwingSFX()
        {
            AudioSource.PlayOneShot(swing);
        }

    }



}



