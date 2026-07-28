using UnityEngine;


namespace PrabuddhaSingh.FinalCharachterController
{
  public class PlayerCombat : MonoBehaviour{
   [SerializeField] private int maxCombo = 2;
   [SerializeField] private float maxAttackGap = 1.2f;

   public int CurrentComboIndex { get; private set; }
   private bool canRecieveInput;
   private bool inputBuffered;
   private float lastAttacktime;

   private PlayerState _playerState;
   private Animator _animator;


    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _animator = GetComponent<Animator>();
        canRecieveInput = true;
    }

    private void Update()
        {
            if(_playerState.CurrentPlayerActionState == PlayerActionStates.Attacking)
            {
                if(Time.time > lastAttacktime + maxAttackGap)
                {
                    Debug.Log("failsafe triggered");
                    ResetCombo();
                }
            }
        }

    public void HandleActionInput(){

        if(_playerState.IsPlayingAction() && CurrentComboIndex >= maxCombo)
        {
           inputBuffered = false;
           return;
        }

        if(_playerState.IsPlayingAction() && !canRecieveInput)
        {
           inputBuffered = true;
           return;     
        }

        if (canRecieveInput)
        {
            StartNextAttack();
        }
        else
        {
            inputBuffered = true;
        }      
    }

    private void StartNextAttack(){

       if(CurrentComboIndex >= maxCombo)
       {
           inputBuffered = false;
           return;
       }

       lastAttacktime = Time.time;
       canRecieveInput = false;
       inputBuffered = false;

       CurrentComboIndex++;
       _animator.SetInteger("AttackIndex", CurrentComboIndex);
       _playerState.SetPlayerActionState(PlayerActionStates.Attacking);    
    }

    public void OpenComboWindow()
    {
        canRecieveInput = true;
        if(inputBuffered)
        {
            StartNextAttack();
        }

    }

    public void CloseComboWindow()
    {
        canRecieveInput = false;
    }

    public void ResetCombo()
    {
        ResetCombatState();
        _playerState.ClearPlayerActionState(); 
    }

    public void ResetCombatState(){
        CurrentComboIndex = 0;
        inputBuffered = false;
        canRecieveInput = true;  

        _animator.SetInteger("AttackIndex", 0);    
    }
}  
}


