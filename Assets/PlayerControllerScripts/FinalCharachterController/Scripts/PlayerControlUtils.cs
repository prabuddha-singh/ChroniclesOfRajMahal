using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace PrabuddhaSingh.FinalCharachterController{
    public class PlayerControlUtils{
         public static Vector3 GetNormalWithSphereCast(CharacterController characterController , LayerMask mask = default){
              Vector3 normal = Vector3.up;
              Vector3 centre = characterController.transform.position + characterController.center;
              float distance = characterController.height / 2f + characterController.stepOffset + 0.01f;

            RaycastHit hit;
            if(Physics.SphereCast(centre ,characterController.radius , Vector3.down, out hit, distance ,  mask)){
                 normal = hit.normal;
            }
            return normal;
         } 
    }
}


