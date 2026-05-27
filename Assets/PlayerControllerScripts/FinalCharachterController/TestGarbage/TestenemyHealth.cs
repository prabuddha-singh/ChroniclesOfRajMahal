using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class TestenemyHealth : MonoBehaviour , IDamageable , IKnockbackable{
    [SerializeField] private float health = 50f;

    private Vector3 knockbackVelovity;
    private float knockbackTimer;

    private Collider _collider;

    public void Awake()
    {
        _collider = GetComponent<Collider>();
    }
 
    public void TakeDamage(float damage)
        {
            health -= damage;
            if(health < 0)
            {
                Destroy(gameObject);
            }
        } 
    
    public void ApplyKnockback(HitInfo hitInfo){
        hitInfo.Direction.y = 0f;
        hitInfo.Direction.Normalize();
        knockbackVelovity = hitInfo.Direction * hitInfo.Force;
        knockbackTimer = hitInfo.Duration;
        _collider.enabled = false;
    }

    public void Update()
        {
            if(knockbackTimer > 0){
                transform.position += knockbackVelovity * Time.deltaTime;
                knockbackTimer -= Time.deltaTime;

                if(knockbackTimer <= 0){
                    _collider.enabled = true;
                }
            }
        }
}
}

