using UnityEngine;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class FootIK : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [Header("IK Settings")]

        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float footOffset = 0.1f;
        [SerializeField] private float ikWeight = 1f;
        [SerializeField] private float rayCastDistace = 1f;
        private bool isikActive = false;

        private void OnAnimatorIK(int layerIndex)
        {
            if(!isikActive)
            {
                Debug.Log("Foot ik is running");
                isikActive = true;
            }
            ;
            if (animator == null) return;

            ApplyFootIK(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot);
            ApplyFootIK(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot);
        }

        private void ApplyFootIK(AvatarIKGoal foot, HumanBodyBones bone)
        {
            Transform footTransform = animator.GetBoneTransform(bone);

            if (footTransform == null) return;

            Vector3 rayOrigin = footTransform.position + Vector3.up * 0.2f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayCastDistace, groundLayer))
            {
                Debug.DrawRay(
                hit.point,
                hit.normal,
                Color.green
                );

                Vector3 footPosition = hit.point;
                footPosition.y += footOffset;
                animator.SetIKPosition(foot, footPosition);
                animator.SetIKPositionWeight(foot, ikWeight);

                Vector3 forward = Vector3.ProjectOnPlane(transform.forward , hit.normal).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(forward, hit.normal);
                animator.SetIKRotationWeight(foot, ikWeight);
                animator.SetIKRotation(foot, targetRotation);
                
            }

            else
            {
                animator.SetIKPositionWeight(foot, 0f);
                animator.SetIKRotationWeight(foot, 0f);
            }
        }

    }
}


