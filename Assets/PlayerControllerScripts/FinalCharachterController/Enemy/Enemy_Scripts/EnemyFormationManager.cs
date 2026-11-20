using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class EnemyFormationManager : MonoBehaviour
    {
        [Header("Front Formation")]
        [SerializeField, Range(30f, 180f)] private float _frontArcAngle = 120f;
        [SerializeField, Min(0.5f)] private float _frontFormationRadius = 2.5f;

        [Header("Waiting Formation")]
        [SerializeField, Min(1f)] private float _waitingRadius = 3.5f;

        [Header("Enemy Spcaing")]
        [SerializeField, Min(0.5f)] private float _minEnemySpacing = 1.5f;

        [Header("Pursuit Separation")]
        [SerializeField, Min(0.5f)] private float _separationRadius = 2f;
        [SerializeField, Min(0.5f)] private float _minimumSeparationDistance = 1.5f;

        [Header("Attack Permission")]
        [SerializeField, Min(1)] private int _maxActiveAttackers = 1;

        private readonly List<EnemyController> _registeredEnemies = new List<EnemyController>();

        public float FrontArcAngle => _frontArcAngle;
        public float FrontFormationRadius => _frontFormationRadius;
        public float WaitingFormationRadius => _waitingRadius;
        public float MinEnemySpacing => _minEnemySpacing;
        public int MaxActiveAttackers => _maxActiveAttackers;

        public IReadOnlyList<EnemyController> RegisteredEnemies => _registeredEnemies;

        [Header("Position Search")]
        [SerializeField, Min(8)] private int _positionSearchSteps = 36;
        [SerializeField, Min(0.1f)] private float _navMeshSampleDistance = 1f;

        private readonly Dictionary<EnemyController, Vector3> _assignedPositions = new();
        private readonly Dictionary<EnemyController, Vector3> _assignedLocalPositions = new();
        private readonly Dictionary<EnemyController, bool> _isWaiting = new();
        private readonly HashSet<EnemyController> _activeAttackers = new();


        private readonly Queue<EnemyController> _attackQueue = new Queue<EnemyController>();

        private readonly HashSet<EnemyController> _queuedEnemies = new HashSet<EnemyController>();

        private readonly HashSet<EnemyController> _attackTurnCompleted = new HashSet<EnemyController>();

        [Header("Attack Queue Settings")]
        [SerializeField, Min(0.5f)] private float _attackStartTimeout = 3f;

        private EnemyController _currentAttackCandidate;
        private float _attackStartTimer;
        private bool _attackStarted;

        private NavMeshPath _navMeshPath;

        [Header("Formation Settings")]
        [SerializeField, Min(0.05f)] private float _formationUpdateInterval = 0.2f;
        private float _formationUpdateTimer = 0f;

        [Header("Formation Rotation")]
        [SerializeField, Range(0f, 90f)] private float _rotationDeadZone = 25f;
        [SerializeField, Min(1f)] private float _formationRotationSpeed = 90f;
        private float _formationYaw;

        [Header("Combat Pacing")]
        [SerializeField, Min(0f)] private float _formationSettleDelay = 1.5f;

        private PlayerController _playerController;
        private float _settleTimer;

        public bool CanEnemiesStartAttack { get; private set; }

        private void Awake()
        {
            _navMeshPath = new NavMeshPath();
            _playerController = GetComponent<PlayerController>();
            _formationYaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            UpdateFormationRotation();
            UpdateCombatReadiness();
            _formationUpdateTimer -= Time.deltaTime;

            if (_formationUpdateTimer >= 0f) return;

            _formationUpdateTimer = _formationUpdateInterval;

            UpdateAssignedPosition();
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null || _registeredEnemies.Contains(enemy)) return;

            _registeredEnemies.Add(enemy);
        }

        public Vector3 GetSeparationVector(EnemyController requestingEnemy)
        {
            if (requestingEnemy == null)
                return Vector3.zero;

            Vector3 separation = Vector3.zero;

            foreach (EnemyController otherEnemy in _registeredEnemies)
            {
                if (otherEnemy == null ||
                    otherEnemy == requestingEnemy ||
                    !otherEnemy.gameObject.activeInHierarchy ||
                    otherEnemy._currentState == EnemyController.EnemyState.Dead)
                {
                    continue;
                }

                Vector3 difference =
                    requestingEnemy.transform.position - otherEnemy.transform.position;

                difference.y = 0f;

                float distance = difference.magnitude;

                if (distance >= _separationRadius)
                    continue;

                Vector3 awayDirection;

                if (distance < 0.001f)
                {
                    int idA = requestingEnemy.GetInstanceID();
                    int idB = otherEnemy.GetInstanceID();

                    int seed = Mathf.Abs(idA ^ idB);
                    float angle = seed % 360;

                    awayDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                    if (idA > idB)
                        awayDirection = -awayDirection;
                }
                else
                {
                    awayDirection = difference / distance;
                }

                float influence = Mathf.InverseLerp(
                    _separationRadius,
                    _minimumSeparationDistance,
                    distance);

                separation += awayDirection * influence;
            }

            return Vector3.ClampMagnitude(separation, 1f);
        }

        public Vector3 GetSeparationTarget(EnemyController requestingEnemy, Vector3 assignedPosition)
        {
            Vector3 separation = GetSeparationVector(requestingEnemy);

            if (separation.sqrMagnitude < 0.001f)
                return assignedPosition;

            Vector3 adjustedPosition =
                assignedPosition + separation * 0.5f;

            NavMeshAgent agent = requestingEnemy.GetComponent<NavMeshAgent>();

            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                return assignedPosition;

            if (!NavMesh.SamplePosition(
                adjustedPosition,
                out NavMeshHit hit,
                0.5f,
                agent.areaMask))
            {
                return assignedPosition;
            }

            bool pathFound = NavMesh.CalculatePath(agent.nextPosition, hit.position, agent.areaMask, _navMeshPath);

            if (!pathFound || _navMeshPath.status != NavMeshPathStatus.PathComplete)
                return assignedPosition;

            return hit.position;
        }

        public bool TryGetAssignedPosition(EnemyController enemy, out Vector3 position, out bool isWaiting)
        {
            position = Vector3.zero;
            isWaiting = false;

            if (enemy == null)
                return false;

            if (_assignedPositions.TryGetValue(enemy, out position))
            {
                isWaiting = _isWaiting.TryGetValue(enemy, out bool waiting) && waiting;
                return true;
            }

            if (TryFindFrontPosition(enemy, out position))
            {
                _assignedPositions.Add(enemy, position);
                _assignedLocalPositions.Add(enemy, Quaternion.Inverse(Quaternion.Euler(0f, _formationYaw, 0f)) * (position - transform.position));
                _isWaiting.Add(enemy, false);
                return true;
            }

            if (TryFindWaitingPosition(enemy, out position))
            {
                _assignedPositions.Add(enemy, position);
                _assignedLocalPositions.Add(enemy, Quaternion.Inverse(Quaternion.Euler(0f, _formationYaw, 0f)) * (position - transform.position));
                _isWaiting.Add(enemy, true);
                isWaiting = true;
                return true;
            }

            return false;
        }

        private bool TryFindFrontPosition(EnemyController enemy, out Vector3 position)
        {
            position = Vector3.zero;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 origin = transform.position;

            float halfArc = _frontArcAngle * 0.5f;
            float bestDistance = float.MaxValue;
            bool found = false;

            for (int i = 0; i < _positionSearchSteps; i++)
            {
                float t = _positionSearchSteps == 1
                    ? 0.5f
                    : (float)i / (_positionSearchSteps - 1);

                float angle = Mathf.Lerp(-halfArc, halfArc, t);

                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;
                Vector3 candidate = origin + direction * _frontFormationRadius;

                if (!TrySamplePosition(enemy, candidate, out Vector3 sampled))
                    continue;

                if (!HasEnoughSpacing(enemy, sampled, false))
                    continue;

                float distance = (sampled - enemy.transform.position).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    position = sampled;
                    found = true;
                }
            }

            return found;
        }

        private bool TryFindWaitingPosition(EnemyController enemy, out Vector3 position)
        {
            position = Vector3.zero;

            Vector3 origin = transform.position;
            float bestDistance = float.MaxValue;
            bool found = false;

            for (int i = 0; i < _positionSearchSteps; i++)
            {
                float angle = i * (360f / _positionSearchSteps);

                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 candidate = origin + direction * _waitingRadius;

                if (!TrySamplePosition(enemy, candidate, out Vector3 sampled))
                    continue;

                if (!HasEnoughSpacing(enemy, sampled, true))
                    continue;

                float distance = (sampled - enemy.transform.position).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    position = sampled;
                    found = true;
                }
            }

            return found;
        }

        private bool TrySamplePosition(EnemyController enemy, Vector3 candidate, out Vector3 sampled)
        {
            sampled = Vector3.zero;

            if (enemy == null) return false;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if (agent == null || !agent.isOnNavMesh || !agent.enabled) return false;

            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                _navMeshSampleDistance,
                agent.areaMask))
                return false;

            sampled = hit.position;

            Vector3 horizontalDifference = sampled - candidate;
            horizontalDifference.y = 0f;

            if (horizontalDifference.sqrMagnitude > Mathf.Pow(0.15f, 2)) return false;

            bool pathFound = NavMesh.CalculatePath(
                agent.nextPosition,
                sampled,
                agent.areaMask,
                _navMeshPath
            );
            if (!pathFound || _navMeshPath.status != NavMeshPathStatus.PathComplete) return false;

            return true;
        }

        private bool HasEnoughSpacing(EnemyController requestingEnemy, Vector3 candidate, bool checkingWaiting)
        {
            float minimumSpacingSqr = _minEnemySpacing * _minEnemySpacing;

            foreach (var assignment in _assignedPositions)
            {
                EnemyController otherEnemy = assignment.Key;

                if (otherEnemy == null || otherEnemy == requestingEnemy)
                    continue;

                if (!_isWaiting.TryGetValue(otherEnemy, out bool otherIsWaiting))
                    continue;

                if (otherIsWaiting != checkingWaiting)
                    continue;

                Vector3 difference = assignment.Value - candidate;
                difference.y = 0f;

                if (difference.sqrMagnitude < minimumSpacingSqr)
                    return false;
            }

            return true;
        }

        private void TryPromotingWaitingEnemy()
        {
            foreach (EnemyController enemy in _registeredEnemies)
            {
                if (enemy == null) continue;

                if (!_isWaiting.TryGetValue(enemy, out bool waiting) || !waiting) continue;

                if (TryFindFrontPosition(enemy, out Vector3 newPosition))
                {
                    _assignedPositions[enemy] = newPosition;
                    _assignedLocalPositions[enemy] = Quaternion.Inverse(Quaternion.Euler(0f, _formationYaw, 0f)) * (newPosition - transform.position);
                    _isWaiting[enemy] = false;

                    return;
                }
            }
        }

        public bool TryRequestAttackPermission(EnemyController enemy)
        {
            if (enemy == null) return false;

            if (_activeAttackers.Contains(enemy)) return true;

            if (!_isWaiting.TryGetValue(enemy, out bool waiting) || waiting) return false;
            
            if(_attackTurnCompleted.Contains(enemy)) return false;

            if(_queuedEnemies.Add(enemy)) _attackQueue.Enqueue(enemy);

            if (_activeAttackers.Count >= _maxActiveAttackers) return false;

            TryGrantNextAttackPermission();

            return _activeAttackers.Contains(enemy);
        }

        private void TryGrantNextAttackPermission()
        {
            if(_activeAttackers.Count >= _maxActiveAttackers) return;

            while(_attackQueue.Count > 0)
            {
                EnemyController nextEnemy = _attackQueue.Dequeue();
                _queuedEnemies.Remove(nextEnemy);

                if(nextEnemy == null ||
                !nextEnemy.gameObject.activeInHierarchy ||
                nextEnemy._currentState == EnemyController.EnemyState.Dead)

                continue;

                if(!_isWaiting.TryGetValue(nextEnemy, out bool waiting)||waiting) continue;

                if(_attackTurnCompleted.Contains(nextEnemy)) continue;

                _activeAttackers.Add(nextEnemy);

                _currentAttackCandidate = nextEnemy;
                _attackStartTimer = _attackStartTimeout;
                _attackStarted = false;

                Debug.Log($"ATTACK GRANTED | {nextEnemy.name} | ID : {nextEnemy.GetInstanceID()}",nextEnemy);

                return;
            }            
            
        }

        private int GetEligibleFrontEnemyCount()
        {
            int count =0;

            foreach(EnemyController enemy in _registeredEnemies)
            {
                if(enemy == null || !enemy.gameObject.activeInHierarchy) continue;

                if(enemy._currentState == EnemyController.EnemyState.Dead) continue;

                if(_isWaiting.TryGetValue(enemy , out bool waiting) && !waiting) count ++;
            }

            return count;
            
        }

        private void RebuildAttackQueue()
        {
            _attackTurnCompleted.Clear();

            foreach(EnemyController enemy in _registeredEnemies)
            {
                if(enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if(enemy._currentState == EnemyController.EnemyState.Dead)
                continue;

                if(!_isWaiting.TryGetValue(enemy,out bool waiting)||waiting)
                continue;

                if(_queuedEnemies.Add(enemy))
                _attackQueue.Enqueue(enemy);
            }
        }

        private void UpdateAssignedPosition()
        {
            foreach (var position in _assignedLocalPositions)
            {
                EnemyController enemy = position.Key;

                if (enemy == null) continue;

                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent == null || !agent.isOnNavMesh || !agent.enabled) continue;

                Vector3 localPosition = position.Value;

                Vector3 rotatedOffset = Quaternion.Euler(0f, _formationYaw, 0f) * localPosition;

                Vector3 candidate = transform.position + rotatedOffset;

                if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    _navMeshSampleDistance,
                    agent.areaMask)) continue;

                Vector3 difference = hit.position - candidate;
                difference.y = 0f;

                if (difference.sqrMagnitude > Mathf.Pow(0.15f, 2)) continue;

                bool pathFound = NavMesh.CalculatePath(
                    agent.nextPosition,
                    hit.position,
                    agent.areaMask,
                    _navMeshPath
                );

                if (!pathFound || _navMeshPath.status != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning(
                      $"FORMATION UPDATE BLOCKED | {enemy.name} | " +
                      $"PathFound: {pathFound} | " +
                      $"PathStatus: {_navMeshPath.status} | " +
                      $"Current: {agent.nextPosition} | " +
                      $"Target: {hit.position}"
                    );

                    continue;

                }
                _assignedPositions[enemy] = hit.position;
            }
        }

        private void UpdateFormationRotation()
        {
            float playerYaw = transform.eulerAngles.y;

            float angleDifference = Mathf.DeltaAngle(
                _formationYaw,
                playerYaw
            );

            if (Mathf.Abs(angleDifference) <= _rotationDeadZone)
                return;

            _formationYaw = Mathf.MoveTowardsAngle(
                _formationYaw,
                playerYaw,
                _formationRotationSpeed * Time.deltaTime
            );
        }

        public void ReleaseAttackPermission(EnemyController enemy)
        {
            if (enemy == null) return;

            bool released = _activeAttackers.Remove(enemy);

            if(!released) return;

            Debug.Log($"ATTACK RELEASED | {enemy.name} | ID: {enemy.GetInstanceID()}",enemy);

            if(_currentAttackCandidate == enemy)
            {
                _currentAttackCandidate = null;
                _attackStartTimer =0f;
                _attackStarted = false;
            }

            _attackTurnCompleted.Add(enemy);

            if(_attackTurnCompleted.Count >= GetEligibleFrontEnemyCount())
            {
                RebuildAttackQueue();
            }

            TryGrantNextAttackPermission();

        }

        public void ReleasePosition(EnemyController enemy)
        {
            if (enemy == null)
                return;

            bool hadPosition = _assignedPositions.Remove(enemy);
            _assignedLocalPositions.Remove(enemy);
            _isWaiting.Remove(enemy);

            if (hadPosition)
            {
                TryPromotingWaitingEnemy();
            }
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy == null || !_registeredEnemies.Contains(enemy)) return;

            ReleaseAttackPermission(enemy);
            _registeredEnemies.Remove(enemy);
            ReleasePosition(enemy);
        }

        private void UpdateCombatReadiness()
        {
            if (_playerController == null)
                return;

            if (_playerController.IsMovingForCombat)
            {
                _settleTimer = 0f;
                CanEnemiesStartAttack = false;
                return;
            }

            _settleTimer += Time.deltaTime;

            CanEnemiesStartAttack = _settleTimer >= _formationSettleDelay;
        }

    }

}
