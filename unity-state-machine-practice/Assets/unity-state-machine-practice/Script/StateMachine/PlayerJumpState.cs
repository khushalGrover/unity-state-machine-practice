using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) :base (currentContext, playerStateFactory) { }

    public override void EnterState() { }

    public override void UpdateState() { }

    public override void ExitState() { }

    public override void CheckSwitchStates() { }

    public override void IntitializeSubState() { }

    void HandleJump()
    {
        if (jumpCount < 3 && currentJumpResetRoutine != null)
        {
            StopCoroutine(currentJumpResetRoutine);
        }
        animator.SetBool(isJumpingHash, true);
        isJumpAnimating = true;
        isJumping = true;
        jumpCount++;
        animator.SetInteger(jumpCountHash, jumpCount);
        currentMovement.y = initialJumpVelocities[jumpCount] * .8f;
        appliedMovement.y = initialJumpVelocities[jumpCount] * .8f;
    }
}

