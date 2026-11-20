using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

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
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private EnemyFormationManager _formationManager;

        [Header("Utility variables")]
        [SerializeField] private float _distance = 1f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _detectionRange = 9f;
        [SerializeField] private float _attackRange = 1.67f;
        [SerializeField] private float _attackCooldown = 0.6f;
        [SerializeField] private float _stoppingDistance = 0.1f;
        [SerializeField] private int _avoidancePriority = 50;
        [SerializeField] private float _disengageRange = 4f;

        [Header("Path Optimization")]
        [SerializeField, Min(0.05f)] private float _pathUpdateInterval = 0.2f;
        [SerializeField, Min(0.1f)] private float _minimumTargetMovement = 0.5f;

        [Header("Attack Movement")]
        [SerializeField] private float _attackLungeDistance = 0.55f;
        [SerializeField] private float _attackLungeDuration = 0.12f;
        [SerializeField] private float _attackRetreatDuration = 0.16f;

        private enum AttackMovementPhase
        {
            None,
            Lunge,
            Hold,
            Retreat
        }

        private AttackMovementPhase _currentMovementPhase;
        private Vector3 _attackStartPosition;
        private Vector3 _attackTargetPosition;
        private float _attackMovementTimer;

        private float attackTimer;
        private float _pathUpdateTimer;
        private Vector3 _lastDestination;

        private bool _isDead = false;
        private bool _playerisDead = false;
        private float _hitLockTimer;
        private bool _isRegistered;
        private bool _hasDestination;

        private float _debugStuckTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            _player = GameObject.FindGameObjectWithTag("Player").transform;
            _playerHealth = _player.GetComponent<PlayerHealth>();
            _playerisDead = _playerHealth.isPlayerDead;

            _enemyAudioController = GetComponent<EnemyAudioController>();
            _enemyAttackHitBox = GetComponentInChildren<EnemyAttackHitbox>();

            _agent.stoppingDistance = _stoppingDistance;
            _agent.avoidancePriority = _avoidancePriority;

            _formationManager = _player.GetComponent<EnemyFormationManager>();
        }

        private void Start()
        {
            ChangeState(EnemyState.Chase);
        }

        private void Update()
        {
            if (_playerisDead) return;

            if (attackTimer > 0f)
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

                if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                }

                return;
            }

            UpdateAttackMovement();
            _distance = Vector3.Distance(transform.position, _player.position);


            if (_currentState == EnemyState.Attack)
            {
                AttackState();
                return;
            }

            UpdateFormationMovement();
        }

        // Handles state transitions only. It does not calculate destinations.
        public void ChangeState(EnemyState newState)
        {
            if (_currentState == newState) return;

            // Release the attack slot when leaving the Attack state.
            if (_currentState == EnemyState.Attack)
            {
                FinishAttackMovement();
                _formationManager?.ReleaseAttackPermission(this);
            }

            _currentState = newState;

            if (newState == EnemyState.Attack)
            {
                attackTimer = _attackCooldown;
            }

            if (newState == EnemyState.Dead)
            {
                DeadState();
            }
        }

        // Handles movement towards the position assigned by EnemyFormationManager.

        private void UpdateFormationMovement()
        {
            if (_formationManager == null) return;
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

            if (!_formationManager.TryGetAssignedPosition(
                this, out Vector3 assignedPosition, out bool isWaiting))
            {
                _agent.isStopped = true;
                return;
            }

            if (_agent.stoppingDistance != _stoppingDistance)
            {
                _agent.stoppingDistance = _stoppingDistance;
            }

            // Update destination for both front and waiting enemies.
            Vector3 movementTarget =
                _formationManager.GetSeparationTarget(this, assignedPosition);

            _pathUpdateTimer -= Time.deltaTime;

            bool destinationChanged =
                !_hasDestination ||
                (_lastDestination - movementTarget).sqrMagnitude >=
                Mathf.Pow(_minimumTargetMovement, 2);

            if (_pathUpdateTimer <= 0f || destinationChanged)
            {
                _agent.SetDestination(movementTarget);

                _lastDestination = movementTarget;
                _pathUpdateTimer = _pathUpdateInterval;
                _hasDestination = true;
            }

            FacePlayer();

            bool reachedDestination =
                !_agent.pathPending &&
                _agent.hasPath &&
                _agent.remainingDistance <= _agent.stoppingDistance + 0.2f;

            // Waiting enemies never request attack permission.
            if (isWaiting)
            {
                if (reachedDestination)
                {
                    _agent.isStopped = true;
                    ChangeState(EnemyState.Idle);
                }
                else
                {
                    _agent.isStopped = false;
                    ChangeState(EnemyState.Chase);
                }

                return;
            }

            // Front-position enemies must reach their assigned positions.
            if (!reachedDestination)
            {
                _agent.isStopped = false;
                ChangeState(EnemyState.Chase);
                return;
            }

            // Enemy has reached its assigned position.
            FacePlayer();
            ChangeState(EnemyState.Idle);

            if (attackTimer > 0f || !_formationManager.CanEnemiesStartAttack)
                return;

            if (_formationManager.TryRequestAttackPermission(this))
            {
                ChangeState(EnemyState.Attack);
            }

            // Temporary navigation diagnostic.
            bool hasMovementTarget =
                !_agent.pathPending &&
                _agent.hasPath &&
                _agent.remainingDistance > _agent.stoppingDistance + 0.2f;

            bool isNotMoving = _agent.velocity.sqrMagnitude < 0.01f;

            if (!hasMovementTarget || isNotMoving || _agent.isStopped)
            {
                _debugStuckTimer += Time.deltaTime;

                if (_debugStuckTimer >= 3f)
                {
                    Debug.LogWarning(
                        $"ENEMY NAV ISSUE: {name} | " +
                        $"State: {_currentState} | " +
                        $"OnNavMesh: {_agent.isOnNavMesh} | " +
                        $"Stopped: {_agent.isStopped} | " +
                        $"PathPending: {_agent.pathPending} | " +
                        $"HasPath: {_agent.hasPath} | " +
                        $"PathStatus: {_agent.pathStatus} | " +
                        $"Remaining: {_agent.remainingDistance} | " +
                        $"Velocity: {_agent.velocity.magnitude:F3}"
                    );

                    _debugStuckTimer = 0f;
                }
            }
            else
            {
                _debugStuckTimer = 0f;
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
            if (_player == null) return;

            Vector3 lookDirection = _player.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.001f) return;

            Quaternion rotationTarget = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotationTarget,
                _rotationSpeed * Time.deltaTime);
        }

        private void IdleState() //0
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }
        }

        private void AttackState() //3
        {
            _agent.isStopped = true;
            FacePlayer();

            if (_distance > _disengageRange)
            {
                _formationManager?.ReleaseAttackPermission(this);

                _hasDestination = false;
                _pathUpdateTimer = 0f;

                ChangeState(EnemyState.Chase);
                return;
            }
        }

        private void BeginAttackMovement()
        {
            Debug.Log($"BEGIN ATTACK MOVEMENT | {name} | State: {_currentState}", this);

            if (_currentState != EnemyState.Attack || _hitLockTimer > 0f) return;

            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

            _attackStartPosition = transform.position;

            Vector3 direction = _player.position - transform.position;
            direction.y = 0f;

            direction.Normalize();

            _attackTargetPosition = transform.position + direction * _attackLungeDistance;

            if (NavMesh.Raycast(_attackStartPosition, _attackTargetPosition, out NavMeshHit hit, NavMesh.AllAreas))
            {
                _attackTargetPosition = hit.position;
            }

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _agent.updatePosition = false;

            _attackMovementTimer = 0f;
            _currentMovementPhase = AttackMovementPhase.Lunge;
        }

        private void BeginAttackRetreat()
        {
            if (_currentMovementPhase == AttackMovementPhase.None || _currentMovementPhase == AttackMovementPhase.Retreat) return;

            _attackMovementTimer = 0f;
            _currentMovementPhase = AttackMovementPhase.Retreat;
        }

        private void UpdateAttackMovement()
        {
            if (_currentMovementPhase == AttackMovementPhase.None) return;

            _attackMovementTimer += Time.deltaTime;

            if (_currentMovementPhase == AttackMovementPhase.Lunge)
            {
                float t = Mathf.Clamp01(_attackMovementTimer / _attackLungeDuration);
                transform.position = Vector3.Lerp(_attackStartPosition, _attackTargetPosition, t);

                if (t >= 1f)
                {
                    _currentMovementPhase = AttackMovementPhase.Hold;
                    _attackMovementTimer = 0f;
                }
            }
            else if (_currentMovementPhase == AttackMovementPhase.Retreat)
            {
                float t = Mathf.Clamp01(_attackMovementTimer / _attackRetreatDuration);
                transform.position = Vector3.Lerp(_attackTargetPosition, _attackStartPosition, t);

                if (t >= 1f)
                {
                    Debug.Log($"RETREAT COMPLETED | {name} | Avoidance: {_agent.obstacleAvoidanceType}", this);
                    _currentMovementPhase = AttackMovementPhase.None;
                    _agent.updatePosition = true;
                    _agent.isStopped = false;
                }
            }

            if (_agent != null && _agent.enabled)
            {
                _agent.nextPosition = transform.position;
            }
        }

        private void FinishAttackMovement()
        {
            Debug.Log($"FINISH ATTACK MOVEMENT | {name} | State: {_currentState}", this);
            _currentMovementPhase = AttackMovementPhase.None;

            if (_agent != null && _agent.enabled)
            {
                _agent.nextPosition = transform.position;
                _agent.updatePosition = true;
                _agent.isStopped = false;
                _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            }
        }

        private void DeadState() //4
        {
            _isDead = true;
            _waveManager?.EnemyDead();
            _formationManager?.ReleaseAttackPermission(this);
            _formationManager?.ReleasePosition(this);

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.enabled = false;
            }
        }

        // Called by the attack animation event when the attack animation finishes.
        public void OnAttackFinished()
        {
            Debug.Log($"ATTACK FINISHED EVENT: {name} | ID: {GetInstanceID()}",this);


            FinishAttackMovement();

            _formationManager?.ReleaseAttackPermission(this);

            _hasDestination = false;
            _pathUpdateTimer = 0f;

            ChangeState(EnemyState.Chase);
        }

        private void OnPlayerDied()
        {
            _playerisDead = true;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }

            ChangeState(EnemyState.Idle);
        }

        private void OnEnable()
        {
            if (_formationManager != null && !_isRegistered)
            {
                _formationManager.RegisterEnemy(this);
                _isRegistered = true;
            }

            if (_enemyAttackHitBox != null)
            {
                _enemyAttackHitBox.OnPlayerHit += HandleEnemyAttackImpactSound;
            }

            if (_playerHealth != null)
            {
                _playerHealth.OnPlayerDeath += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (_formationManager != null && _isRegistered)
            {
                _formationManager.UnregisterEnemy(this);
                _isRegistered = false;
            }

            if (_enemyAttackHitBox != null)
            {
                _enemyAttackHitBox.OnPlayerHit -= HandleEnemyAttackImpactSound;
            }

            if (_playerHealth != null)
            {
                _playerHealth.OnPlayerDeath -= OnPlayerDied;
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