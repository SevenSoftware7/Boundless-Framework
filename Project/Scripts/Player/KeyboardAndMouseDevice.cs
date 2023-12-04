using System.Collections.Generic;
using Godot;
using SevenGame.Utility;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class KeyboardAndMouseDevice : ControlDevice {

    public KeyInputInfo moveForwardKeyEvent;
    public KeyInputInfo moveBackwardKeyEvent;
    public KeyInputInfo moveLeftKeyEvent;
    public KeyInputInfo moveRightKeyEvent;
    public KeyInputInfo jumpKeyEvent;
    public KeyInputInfo sprintKeyEvent;
    public Vector2Info moveMotion;
    public Vector2Info mouseMotion;

    private static readonly Dictionary<Key, Key> overrideKeyMap = new();

    

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
        moveForwardKeyEvent.TimeStep();
        moveBackwardKeyEvent.TimeStep();
        moveLeftKeyEvent.TimeStep();
        moveRightKeyEvent.TimeStep();
        jumpKeyEvent.TimeStep();
        sprintKeyEvent.TimeStep();

        mouseMotion.TimeStep();
    }



    public override void _Process(double delta) {
        base._Process(delta);

        if ( Engine.IsEditorHint() ) return;

        CallDeferred(MethodName.KeyInfoTimeStep);
    }

    public override void _Input(InputEvent @event) {
        base._Input(@event);
        
        if (@event is InputEventMouseMotion mouseMotion) {
            this.mouseMotion.SetVal(new(
                mouseMotion.Relative.X,
                -mouseMotion.Relative.Y
            ));
            return;
        }

        if (@event is InputEventKey keyEvent) {
            Key keyCode = keyEvent.PhysicalKeycode;
            if ( overrideKeyMap.ContainsKey(keyCode) ) {
                keyCode = overrideKeyMap[keyCode];
            }

            switch (keyCode) {
                case Key.W: moveForwardKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); break;
                case Key.S: moveBackwardKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); break;
                case Key.A: moveLeftKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); break;
                case Key.D: moveRightKeyEvent.SetVal(keyEvent.Pressed); UpdateMoveMotion(); break;
                case Key.Space: jumpKeyEvent.SetVal(keyEvent.Pressed); break;
                case Key.Shift: sprintKeyEvent.SetVal(keyEvent.Pressed); break;
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
