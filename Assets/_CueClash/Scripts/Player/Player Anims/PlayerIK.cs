using UnityEngine;

public class PlayerIK : MonoBehaviour
{
    [SerializeField] private Transform headLookAt;
    [SerializeField] private Transform handAim;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKPosition(AvatarIKGoal.RightHand, handAim.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, handAim.rotation);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, handAim.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, handAim.rotation);

        animator.SetLookAtWeight(1, 0, 1, 0, 0);
        animator.SetLookAtPosition(headLookAt.position);
    }
}
