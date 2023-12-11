using System.Collections.Generic;
using Godot;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class KeyboardAndMouseDevice : ControlDevice {

    public KeyInputInfo moveForwardKeyEvent;
    public bool moveForwardKeyUpdated;
    
    public KeyInputInfo moveBackwardKeyEvent;
    public bool moveBackwardKeyUpdated;
    
    public KeyInputInfo moveLeftKeyEvent;
    public bool moveLeftKeyUpdated;
    
    public KeyInputInfo moveRightKeyEvent;
    public bool moveRightKeyUpdated;
    
    public KeyInputInfo jumpKeyEvent;
    public bool jumpKeyUpdated;
    
    public KeyInputInfo sprintKeyEvent;
    public bool sprintKeyUpdated;

    public Vector2Info mouseMotion;
    public bool mouseMotionUpdated;
    
    public Vector2Info moveMotion;

    

    private static readonly Dictionary<Key, Key> overrideKeyMap = [];

    

    private static Vector2 GetVector(
        KeyInputInfo upKey,
        KeyInputInfo downKey,
        KeyInputInfo leftKey,
        KeyInputInfo rightKey,
        float? clampMagnitude = null
    ) {
        Vector2 result = new(
            (rightKey.currentValue ? 1 : 0) - (leftKey.currentValue ? 1 : 0),
            (downKey.currentValue ? 1 : 0) - (upKey.currentValue ? 1 : 0)
        );
        
        return clampMagnitude.HasValue ? result.ClampMagnitude(clampMagnitude.Value) : result;
    }


    public override Vector2Info GetLookDirection() {
        return mouseMotion;
    }

    public override Vector2Info GetMoveDirection() {
        return moveMotion;
    }

    public override KeyInputInfo GetJumpInput() {
        return jumpKeyEvent;
    }

    public override KeyInputInfo GetSprintInput() {
        return sprintKeyEvent;
    }



    private void KeyInfoTimeStep() {
        if ( ! moveForwardKeyUpdated ) moveForwardKeyEvent.SetVal(moveForwardKeyEvent.currentValue); 
        moveForwardKeyUpdated = true;

        if ( ! moveBackwardKeyUpdated ) moveBackwardKeyEvent.SetVal(moveBackwardKeyEvent.currentValue); 
        moveBackwardKeyUpdated = true;

        if ( ! moveLeftKeyUpdated ) moveLeftKeyEvent.SetVal(moveLeftKeyEvent.currentValue); 
        moveLeftKeyUpdated = true;

        if ( ! moveRightKeyUpdated ) moveRightKeyEvent.SetVal(moveRightKeyEvent.currentValue); 
        moveRightKeyUpdated = true;

        if ( ! jumpKeyUpdated ) jumpKeyEvent.SetVal(jumpKeyEvent.currentValue); 
        jumpKeyUpdated = true;

        if ( ! sprintKeyUpdated ) sprintKeyEvent.SetVal(sprintKeyEvent.currentValue); 
        sprintKeyUpdated = true;


        if ( ! mouseMotionUpdated ) mouseMotion.SetVal(Vector2.Zero); 
        mouseMotionUpdated = true;

    }



    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) return;

        Callable.From(KeyInfoTimeStep).CallDeferred();
    }

    public override void _Input(InputEvent @event) {
        base._Input(@event);
        
        if (@event is InputEventMouseMotion mouseMotion) {
            this.mouseMotion.SetVal(new(
                mouseMotion.Relative.X,
                -mouseMotion.Relative.Y
            ));
            mouseMotionUpdated = true;
            return;
        }

        if (@event is InputEventKey keyEvent) {
            Key keyCode = keyEvent.PhysicalKeycode;
            if ( overrideKeyMap.TryGetValue(keyCode, out Key value)) {
                keyCode = value;
            }

            switch (keyCode) {
                case Key.W: moveForwardKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); moveForwardKeyUpdated = true; break;
                case Key.S: moveBackwardKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); moveBackwardKeyUpdated = true; break;
                case Key.A: moveLeftKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); moveLeftKeyUpdated = true; break;
                case Key.D: moveRightKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); moveRightKeyUpdated = true; break;
                case Key.Space: jumpKeyEvent.SetVal(keyEvent.Pressed); jumpKeyUpdated = true; break;
                case Key.Shift: sprintKeyEvent.SetVal(keyEvent.Pressed); sprintKeyUpdated = true; break;
            }

            void UpdateMoveMotion() {
                moveMotion.SetVal(
                    GetVector(
                        moveForwardKeyEvent,
                        moveBackwardKeyEvent,
                        moveLeftKeyEvent,
                        moveRightKeyEvent,
                        1f
                    )
                );
            }

            return;
        }
    }
}
