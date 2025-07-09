using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace PrabuddhaSingh.FinalCharachterController{
    public class ThirdPersonActions : MonoBehaviour , PlayerControls.IThirdPersonMapActions{
      
    public PlayerControls PlayerControls { get; private set; }
    
    private void OnEnable(){
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.ThirdPersonMap.Enable();
        PlayerControls.ThirdPersonMap.SetCallbacks(this);
    }

    private void OnDisable(){
        PlayerControls.ThirdPersonMap.Disable();
        PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
    }

        

        public void OnMouseScroll(InputAction.CallbackContext context)
        {
           if(!context.performed){
            return;
           }
           Vector2 scrollInput = context.ReadValue<Vector2>();
           Debug.Log(scrollInput);
        }
    }
}



