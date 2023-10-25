/*
 * Abstract State 
 * 
 * Establishes methods and variables that concrete states will inherit when derived from this class 
 * 
 */

public abstract class PlayerBaseState 
{
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) { 
        _ctx = currentContext;
        _factory = playerStateFactory;  
    }


    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchStates();

    public abstract void IntitializeSubState();

    void UpdateStates() { }

    protected void SwitchState(PlayerBaseState newState) 
    {
        // current state exits state
        ExitState();

        // new State enters state
        newState.EnterState();

        // Switch current state of context
        _ctx.CurrentState = newState;
    }

    protected void SetSuperState() 
    {
    
    }

    protected void SetSubState() 
    {
    
    }


}
