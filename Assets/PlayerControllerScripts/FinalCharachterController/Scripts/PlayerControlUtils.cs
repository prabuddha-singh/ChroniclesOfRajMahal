using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
//using UnityEngine.ProBuilder;

namespace PrabuddhaSingh.FinalCharachterController{
    public class PlayerControlUtils{
        public static bool CheckGrounded(
               CharacterController controller,
               out Vector3 groundNormal)
          {
    
             groundNormal = Vector3.up;

             LayerMask mask = LayerMask.GetMask("Default");

              Vector3 centre = controller.transform.position + controller.center;
              float distance = controller.height / 2f + controller.stepOffset + 0.09f;

           if (Physics.SphereCast(
               centre,
               controller.radius,
               Vector3.down,
               out RaycastHit hit,
               distance,
               mask,
               QueryTriggerInteraction.Ignore))
          {
               groundNormal = hit.normal;
               return true;
          }

      return false;
     }

    }
}


