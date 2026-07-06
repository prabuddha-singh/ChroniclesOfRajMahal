using System.Collections.Generic;
using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class AttackHitbox : MonoBehaviour{
    [SerializeField] private Collider _collider;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectLifetime = 1f;
    private bool _hasHit;
    private PlayerState _playerState;

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _playerState = GetComponentInParent<PlayerState>();
        _collider.enabled = false;
    }

    public void EnableHitbox()
    {
        _hasHit = false;
        _collider.enabled = true;
        hitTargets.Clear();
    }

    public void DisableHitbox()
    {
        _collider.enabled = false;
        _hasHit=false;
        hitTargets.Clear();
    }

    private void SpawnHitEffect(Vector3 position , Vector3 normal)
        {
            if(hitEffectPrefab == null) return;

            Quaternion rotation = Quaternion.LookRotation(normal);
            GameObject HitVFX = Instantiate(hitEffectPrefab, position, rotation);
            Destroy(HitVFX , hitEffectLifetime);
        }

    private void OnTriggerEnter(Collider other)
        {
            if(_hasHit) return;

            if(!_playerState.IsPlayingAction()) return;

            if(other.CompareTag("Player")) return;

            if(hitTargets.Contains(other.gameObject)) return;

            hitTargets.Add(other.gameObject);

            if(other.TryGetComponent<IDamageable>(out var damageable))
            {
                _hasHit = true;
                damageable.TakeDamage(10);
                Debug.Log("Dealt damage to " + other.name); 
            }

            if(other.TryGetComponent<IKnockbackable>(out var knockbackable))
            {
                HitInfo hitInfo = new HitInfo
                {
                    Direction = (other.transform.position - transform.position).normalized,
                    Force = knockbackForce,
                    Duration = knockbackDuration
                };
                knockbackable.ApplyKnockback(hitInfo);
            }
            
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (transform.position - closestPoint).normalized;
            
            SpawnHitEffect(closestPoint, hitNormal);
            if(HitStopManager.instance != null)
            {
                HitStopManager.instance.DoHitStop(0.08f);
            }
            else
            {
                Debug.Log("no trigger(hitstop)");
            }
            if(CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.35f, 1.50f);
            }
            else
            {
                Debug.Log("no trigger(camerashake)");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            hitTargets.Remove(other.gameObject);
        }
   
   
  } 
}
   



