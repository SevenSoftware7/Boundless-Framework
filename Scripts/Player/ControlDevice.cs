using Godot;
using SevenGame.Utility;
using System;


namespace EndlessSkies.Core;

public abstract partial class ControlDevice : Node {
    
    public abstract Vector2 GetLookDirection();
    public abstract Vector2 GetMoveDirection();
    public abstract KeyInputInfo GetJumpKey();
    public abstract KeyInputInfo GetSprintKey();

}
