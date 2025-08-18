using UnityEngine;

public class AttackVFXRelay : StateMachineBehaviour
{
    private AttackVFXController controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null)
            controller = animator.GetComponentInChildren<AttackVFXController>(true);

        controller?.OnStateEnterHash(stateInfo.shortNameHash);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null)
            controller = animator.GetComponentInChildren<AttackVFXController>(true);

        controller?.OnStateExitHash(stateInfo.shortNameHash);
    }
}
