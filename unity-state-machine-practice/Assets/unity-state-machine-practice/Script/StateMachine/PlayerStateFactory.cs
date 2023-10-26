/*
 * Constact Function are called 
 * when we create a new instance
 * of a class
 */

public class PlayerStateFactory 
{
    PlayerStateMachine _context;
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    public PlayerBaseState Idle() 
    {
        return new PlayerIdleState(_context, this);
    }
    public PlayerBaseState Walk() 
    {
        return new PlayerWalkState(_context, this);
    }
    public PlayerBaseState Run() 
    {
        return new PlayerRunState(_context, this);
    }
    public PlayerBaseState Jump() 
    {
        return new PlayerJumpState(_context, this);
    }
    public PlayerBaseState Grounded() 
    {
        return new PlayerGroundState(_context, this);
    }
}
