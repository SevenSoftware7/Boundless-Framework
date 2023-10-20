using Godot;
using SevenGame.Utility;
using System;


namespace LandlessSkies.Core;

public abstract partial class ControlDevice : Node {
    
    public abstract Vector2Info GetLookDirection();
    public abstract Vector2Info GetMoveDirection();
    public abstract KeyInputInfo GetJumpInput();
    public abstract KeyInputInfo GetSprintInput();

}
