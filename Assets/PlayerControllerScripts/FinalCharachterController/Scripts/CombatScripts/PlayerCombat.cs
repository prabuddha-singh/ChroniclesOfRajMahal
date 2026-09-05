using UnityEngine;


namespace PrabuddhaSingh.FinalCharachterController
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private int maxCombo = 2;
        [SerializeField] private float maxAttackGap = 1.2f;

        public int CurrentComboIndex { get; private set; }
        private bool canReceiveInput;
        private bool inputBuffered;
        private float lastAttacktime;

        private PlayerState _playerState;
        private Animator _animator;
        private PlayerCombatAudio _playerCombatAudio;
        private AttackHitbox attackHitbox;


        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();
            _animator = GetComponent<Animator>();
            _playerCombatAudio = GetComponent<PlayerCombatAudio>();
            attackHitbox = GetComponentInChildren<AttackHitbox>();
            canReceiveInput = true;
        }

        private void Update()
        {
            if (_playerState.CurrentPlayerActionState == PlayerActionStates.Attacking)
            {
                if (Time.time > lastAttacktime + maxAttackGap)
                {
                    ResetCombo();
                }
            }
        }

        private void OnEnable()
        {
            if(attackHitbox != null)
            {
                Debug.Log("Subscribing to OnHit event");
                attackHitbox.OnHit += HandleAttackImpactSound;
            }
        }

        private void OnDisable()
        {
            if(attackHitbox != null)
            {
                Debug.Log("Unsubscribing from OnHit event");
                attackHitbox.OnHit -= HandleAttackImpactSound;
            }
        }

        private void HandleAttackImpactSound()
        {
            Debug.Log("Impact sound triggered");
           _playerCombatAudio.PlayImpactSFX(CurrentComboIndex);
        }

        public void HandleActionInput()
        {

            if (_playerState.IsPlayingAction() && CurrentComboIndex >= maxCombo)
            {
                inputBuffered = false;
                return;
            }

            if (_playerState.IsPlayingAction() && !canReceiveInput)
            {
                inputBuffered = true;
                return;
            }

            if (canReceiveInput)
            {
                StartNextAttack();
            }
            else
            {
                inputBuffered = true;
            }
        }

        private void StartNextAttack()
        {

            if (CurrentComboIndex >= maxCombo)
            {
                inputBuffered = false;
                return;
            }

            lastAttacktime = Time.time;
            canReceiveInput = false;
            inputBuffered = false;

            CurrentComboIndex++;
            _animator.SetInteger("AttackIndex", CurrentComboIndex);
            _playerState.SetPlayerActionState(PlayerActionStates.Attacking);
        }

        public void OpenComboWindow()
        {
            canReceiveInput = true;
            if (inputBuffered)
            {
                StartNextAttack();
            }

        }

        public void CloseComboWindow()
        {
            canReceiveInput = false;
        }

        public void ResetCombo()
        {
            ResetCombatState();
            _playerState.ClearPlayerActionState();
        }

        public void CleanupAfterAttackExit(int attackIndex)
        {
            if (CurrentComboIndex > attackIndex)
            {
                return;
            }

            ResetCombatState();

            if (_playerState.CurrentPlayerActionState == PlayerActionStates.Attacking)
            {
                _playerState.ClearPlayerActionState();
            }
        }

        public void ResetCombatState()
        {
            CurrentComboIndex = 0;
            inputBuffered = false;
            canReceiveInput = true;

            _animator.SetInteger("AttackIndex", 0);
        }
    }
}
