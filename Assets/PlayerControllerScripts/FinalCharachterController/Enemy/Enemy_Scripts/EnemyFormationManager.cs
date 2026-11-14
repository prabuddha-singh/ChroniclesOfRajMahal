using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

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
        private NavMeshPath _navMeshPath;

        [Header("Formation Settings")]
        [SerializeField, Min(0.05f)] private float _formationUpdateInterval = 0.2f;
        private float _formationUpdateTimer = 0f;

        private void Awake()
        {
            _navMeshPath = new NavMeshPath();
        }

        private void Update()
        {
            _formationUpdateTimer -= Time.deltaTime;

            if(_formationUpdateTimer >= 0f) return;

            _formationUpdateTimer = _formationUpdateInterval;

            UpdateAssignedPosition();
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null || _registeredEnemies.Contains(enemy)) return;

            _registeredEnemies.Add(enemy);
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
                _assignedLocalPositions.Add(enemy, transform.InverseTransformPoint(position));
                _isWaiting.Add(enemy, false);
                return true;
            }

            if (TryFindWaitingPosition(enemy, out position))
            {
                _assignedPositions.Add(enemy, position);
                _assignedLocalPositions.Add(enemy, transform.InverseTransformPoint(position));
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

                if (!HasEnoughSpacing(enemy, sampled))
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

                if (!HasEnoughSpacing(enemy, sampled))
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

            if(enemy == null) return false;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if(agent == null || !agent.isOnNavMesh || !agent.enabled) return false;

            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                _navMeshSampleDistance,
                agent.areaMask))
                return false;

            sampled = hit.position;

            Vector3 horizontalDifference = sampled - candidate;
            horizontalDifference.y =0f;

            if(horizontalDifference.sqrMagnitude > Mathf.Pow(0.15f,2)) return false;

            bool pathFound = NavMesh.CalculatePath(
                agent.nextPosition,
                sampled,
                agent.areaMask,
                _navMeshPath
            );
            if(!pathFound || _navMeshPath.status != NavMeshPathStatus.PathComplete) return false;
            
            return true;
        }

        private bool HasEnoughSpacing(
            EnemyController requestingEnemy,
            Vector3 candidate)
        {
            float minimumSpacingSqr = _minEnemySpacing * _minEnemySpacing;

            foreach (var otherEnemy in _registeredEnemies)
            {
                if(otherEnemy == null || otherEnemy == requestingEnemy) continue;

                Vector3 difference = otherEnemy.transform.position - candidate;
                difference.y=0;

                if(difference.sqrMagnitude < minimumSpacingSqr) return false;
            }

            foreach (var assignment in _assignedPositions)
            {
                if (assignment.Key == null || assignment.Key == requestingEnemy)
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
               if(enemy == null) continue;

               if(!_isWaiting.TryGetValue(enemy, out bool waiting) || !waiting) continue;

               if(TryFindFrontPosition(enemy, out Vector3 newPosition))
                {
                    _assignedPositions[enemy] = newPosition;
                    _assignedLocalPositions[enemy] = transform.InverseTransformPoint(newPosition);
                    _isWaiting[enemy] = false;

                    return;
                }
            }
        }

        public bool TryRequestAttackPermission(EnemyController enemy)
        {
            if(enemy == null) return false;

            if(_activeAttackers.Contains(enemy)) return true;

            if(!_isWaiting.TryGetValue(enemy, out bool waiting) || waiting) return false;

            if(_activeAttackers.Count >= _maxActiveAttackers) return false;

            _activeAttackers.Add(enemy);
            return true;
        }

        private void UpdateAssignedPosition()
        {
            foreach( var position in _assignedLocalPositions)
            {
                EnemyController enemy = position.Key;

                if(enemy == null) continue;

                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if(agent == null || !agent.isOnNavMesh || !agent.enabled) continue;

                Vector3 candidate = transform.TransformPoint(position.Value);

                if(!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    _navMeshSampleDistance,
                    agent.areaMask)) continue;

                Vector3 difference = hit.position - candidate;
                difference.y = 0f;

                if(difference.sqrMagnitude > Mathf.Pow(0.15f,2)) continue;

                bool pathFound = NavMesh.CalculatePath(
                    agent.nextPosition,
                    hit.position,
                    agent.areaMask,
                    _navMeshPath
                );

                _assignedPositions[enemy] = hit.position;
            }
        }

        public void ReleaseAttackPermission(EnemyController enemy)
        {
            if(enemy == null) return;

            _activeAttackers.Remove(enemy);
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

    }

}
