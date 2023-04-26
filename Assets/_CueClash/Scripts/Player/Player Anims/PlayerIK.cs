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
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, _handAim.position);
        _animator.SetIKRotation(AvatarIKGoal.RightHand, _handAim.rotation);

        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _handAim.position);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, _handAim.rotation);

        animator.SetLookAtWeight(1, 0, 1, 0, 0);
        animator.SetLookAtPosition(headLookAt.position);
    }
}
