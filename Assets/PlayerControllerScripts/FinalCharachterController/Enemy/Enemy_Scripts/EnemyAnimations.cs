using UnityEngine;
namespace PrabuddhaSingh.FinalCharachterController
{
public class EnemyAnimations : MonoBehaviour{
    
    [SerializeField] private Animator _animator;
    private EnemyController _enemyController;

    int StateHash = Animator.StringToHash("State");

    private void Awake(){
        _animator = GetComponent<Animator>();
        _enemyController = GetComponent<EnemyController>();  
    }

    private void Update(){
       _animator.SetInteger(StateHash,(int)_enemyController._currentState);     
    }
}

}
