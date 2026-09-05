using UnityEngine;
using UnityEngine.AI;


namespace PrabuddhaSingh.FinalCharachterController
{
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyState
        {
            Idle = 0,
            Patrol = 1,
            Chase = 2,
            Attack = 3,
            Dead = 4
        }

        [Header("components")]
        [SerializeField] public EnemyState _currentState;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Transform _player;
        [SerializeField] private EnemyAudioController _enemyAudioController;
        [SerializeField] private EnemyAttackHitbox _enemyAttackHitBox;
        [SerializeField] private WaveManager _waveManager;


        [Header("Utility variables")]
        [SerializeField] private float _distance = 1f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _detectionRange = 9f;
        [SerializeField] private float _attackRange = 1.67f;//Six Seveeeeen
        [SerializeField] private float _attackCooldown = 0.6f;
        [SerializeField] private float _stoppingDistance =1.2f;
        [SerializeField] private int _avoidancePriority = 50;
        private float attackTimer;

        private bool _isDead = false;
        private float _hitLockTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            _enemyAudioController = GetComponent<EnemyAudioController>();
            _enemyAttackHitBox = GetComponentInChildren<EnemyAttackHitbox>();
            _agent.stoppingDistance = _stoppingDistance;
            _agent.avoidancePriority = _avoidancePriority;
        }

        private void Start()
        {
            ChangeState(EnemyState.Chase);
        }

        private void Update()
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            if (_isDead)
            {
                ChangeState(EnemyState.Dead);
                return;
            }

            if (_hitLockTimer > 0f)
            {
                _hitLockTimer -= Time.deltaTime;
                _agent.isStopped = true;
                return;
            }

            _distance = Vector3.Distance(transform.position, _player.position);

            if (_distance > _detectionRange)
            {
                ChangeState(EnemyState.Idle);
            }
            else if (_distance > _attackRange)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                if (attackTimer <= 0f)
                {
                    ChangeState(EnemyState.Attack);
                }
            }

            switch (_currentState)
            {
                case EnemyState.Idle:
                    IdleState();
                    break;
                case EnemyState.Chase:
                    ChaseState();
                    break;
                case EnemyState.Dead:
                    DeadState();
                    break;
                case EnemyState.Attack:
                    AttackState();
                    break;
            }

        }

        public void ChangeState(EnemyState newState)
        {
            if (_isDead && newState != EnemyState.Dead) return;
            if (_currentState == newState) return;
            _currentState = newState;

            if (newState == EnemyState.Dead)
            {
                _isDead = true;

                _waveManager?.EnemyDead();
            }
        }

        public bool TryPlayHitReaction(float duration)
        {
            if (_isDead || _hitLockTimer > 0f) return false;

            _hitLockTimer = duration;
            ChangeState(EnemyState.Idle);
            return true;
        }

        private void FacePlayer()
        {
            Vector3 lookDirection = _player.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.001f) return;

            Quaternion rotationTarget = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, _rotationSpeed * Time.deltaTime);
        }
        private void IdleState() //0
        {
            _agent.isStopped = true;
        }

        private void ChaseState() //2
        {
            //FacePlayer();
            _agent.isStopped = false;
            if(_agent.stoppingDistance != _stoppingDistance)
            {
                _agent.stoppingDistance = _stoppingDistance;
            }
            _agent.SetDestination(_player.position);
        }

        private void AttackState()
        { //3
            FacePlayer();
            _agent.isStopped = true;
            if (attackTimer <= 0) attackTimer = _attackCooldown;
        }

        private void DeadState()
        { //4
            _isDead = true;
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.enabled = false;
            }
        }

        public void OnAttackFinished()
        {

            ChangeState(EnemyState.Chase);

        }

        public void OnEnable()
        {
            if (_enemyAttackHitBox != null)
            {
                _enemyAttackHitBox.OnPlayerHit += HandleEnemyAttackImpactSound;
            }
        }

        public void OnDisable()
        {
            if (_enemyAttackHitBox != null)
            {
                _enemyAttackHitBox.OnPlayerHit -= HandleEnemyAttackImpactSound;
            }
        }

        private void HandleEnemyAttackImpactSound()
        {
            _enemyAudioController.PlayImpactSFX();
        }

        public void SetWaveManager(WaveManager waveManager)
        {
            _waveManager = waveManager;
        }

    }
}
