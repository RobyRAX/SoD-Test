using RAXY.Movement;
using Sirenix.OdinInspector;

public class UnitMovement : MovementController
{
    public float walkSpeedModifier = 2f;
    public float runSpeedModifier = 5f;

    public float jumpHeight = 7.5f;
    public float doubleJumpHeight = 5f;

    bool _wasJump;
    [ShowInInspector]
    public bool WasJump
    {
        get => _wasJump;
        set
        {
            if (value == _wasJump)
                return;

            _wasJump = value;
        }
    }

    bool _wasDoubleJump;
    [ShowInInspector]
    public bool WasDoubleJump
    {
        get => _wasDoubleJump;
        set
        {
            if (value == _wasDoubleJump)
                return;

            _wasDoubleJump = value;
        }
    }
}