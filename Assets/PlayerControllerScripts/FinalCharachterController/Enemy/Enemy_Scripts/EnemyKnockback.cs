using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
   public class EnemyKnockback : MonoBehaviour , IKnockbackable{
    private Vector3 _knockbackVelocity;
    private float _knockbackTimer;

    [SerializeField] private float knockbackDrag=10f;

    private Animator _animator;

    private bool _chachedRootMotionState;

    private void Awake()
    {
       _animator = GetComponent<Animator>();     
    }

    public void ApplyKnockback(HitInfo hitInfo)
    {
        if(_knockbackTimer > 0f) return;

        hitInfo.Direction.y = 0f;
        hitInfo.Direction.Normalize();

        _knockbackVelocity = hitInfo.Direction * hitInfo.Force;
        _knockbackTimer = hitInfo.Duration;

        _chachedRootMotionState = _animator.applyRootMotion;
        _animator.applyRootMotion = false;
    }

    private void Update()
    {
        if(_knockbackTimer <= 0f) return;

        _knockbackTimer -= Time.deltaTime;
        transform.position += _knockbackVelocity * Time.deltaTime;

        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity,Vector3.zero, knockbackDrag * Time.deltaTime);
        if(_knockbackTimer <= 0f)
        {
            _animator.applyRootMotion = _chachedRootMotionState;
        }
    }

} 
}


