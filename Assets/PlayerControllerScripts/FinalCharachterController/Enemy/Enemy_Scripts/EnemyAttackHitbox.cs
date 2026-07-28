using UnityEngine;
using System.Collections.Generic;

namespace PrabuddhaSingh.FinalCharachterController
{
public class EnemyAttackHitbox : MonoBehaviour{

    [SerializeField] private Collider _collider;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectLifetime = 1f;
    private bool _hasHit;

    [SerializeField] private EnemyController _enemyController;

    private HashSet<GameObject> hitTargets =new HashSet<GameObject>();

    private void Awake(){
       _collider = GetComponent<Collider>();
       _collider.enabled = false;

       _enemyController = GetComponentInParent<EnemyController>();
    }

    public void EnableEnemyHitBox(){
      _hasHit = false;
      _collider.enabled = true;
      hitTargets.Clear();        
    }

    public void DisableEnemyHitBox(){
        _collider.enabled = false;
        _hasHit = false;
        hitTargets.Clear();
    }

    private void SpawnHitEffect(Vector3 position,Vector3 normal){
        if(hitEffectPrefab == null) return;

        Quaternion rotation = Quaternion.LookRotation(normal);
        GameObject HitVFX = Instantiate(hitEffectPrefab,position,rotation);  
        Destroy(HitVFX,hitEffectLifetime);
    }

    private void OnTriggerEnter(Collider other){
        Debug.Log($"Enemy hitbox triggered by {other.gameObject.name}");
       if(_hasHit) return;

       if(_enemyController._currentState != EnemyController.EnemyState.Attack) return;

       if(other.CompareTag("Enemy")) return;

       if(hitTargets.Contains(other.gameObject)) return;

       hitTargets.Add(other.gameObject);

       if(other.TryGetComponent<IDamageable>(out var damageable)){
          _hasHit = true;
          damageable.TakeDamage(10);
        }
       if(other.TryGetComponent<IKnockbackable>(out var knockbackable)){
          Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
          HitInfo hitInfo =new HitInfo{
            Direction = knockbackDirection,
            Force = knockbackForce,
            Duration = knockbackDuration
          };
          knockbackable.ApplyKnockback(hitInfo);
        }

        Vector3 closestPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (closestPoint - transform.position).normalized;

        SpawnHitEffect(closestPoint,hitNormal);

        if(HitStopManager.instance !=null){
            HitStopManager.instance.DoHitStop(0.09f);
        }
        else{
            Debug.Log("Hitstop instance null");    
        }

        if(CameraShake.Instance != null){
            CameraShake.Instance.Shake(0.35f,1.50f);    
        }
        else{
            Debug.Log("Camera shake instance null");
        }


    }

     private void OnTriggerExit(Collider other){
      hitTargets.Remove(other.gameObject);        
    }

    }
}
