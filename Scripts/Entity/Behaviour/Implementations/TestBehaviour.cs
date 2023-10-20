using Godot;
using SevenGame.Utility;
using System;


namespace LandlessSkies.Core;

public partial class TestBehaviour : EntityBehaviour {

    private Vector3 _moveDirection;
    private Vector3 _rotationForward = Vector3.Forward;
    private float _moveSpeed;
    private MovementSpeed _movementSpeed;



    public override void Start(EntityBehaviour previousBehaviour) {
        base.Start(previousBehaviour);

        Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
        _rotationForward = Entity.Transform.Basis.Z;
    }

    public override void HandleInput(Player.InputInfo inputInfo) {
        base.HandleInput(inputInfo);

        if (inputInfo.ControlDevice.GetJumpInput()) {
            Jump();
        }
        
        inputInfo.RawInputToGroundedMovement(camRotation: out _, out Vector3 groundedMovement);

        float speedSquared = groundedMovement.LengthSquared();
        MovementSpeed speed = speedSquared switch {
            _ when Mathf.IsZeroApprox(speedSquared)             => MovementSpeed.Idle,
            _ when speedSquared <= 0.25f                        => MovementSpeed.Walk,
            _ when inputInfo.ControlDevice.GetSprintInput()     => MovementSpeed.Sprint,
            _                                                   => MovementSpeed.Run
        };
        SetSpeed(speed);

        Move(groundedMovement);
    }

    public override void SetSpeed(MovementSpeed speed) {
        base.SetSpeed(speed);

        if ( speed == _movementSpeed ) return;
        
        bool isBusyEvading = /* Entity.Behaviour.EvadeBehaviour?.state ??  */false;
        if (Entity.IsOnFloor() && ! isBusyEvading) {
            if ( speed == MovementSpeed.Idle ) {

                // MovementStopAnimation();
            
            } else if ( (int)speed > (int)_movementSpeed ) {

                // MovementStartAnimation(speed);

            }
        }
        _movementSpeed = speed;
    }

    public override void Move(Vector3 direction) {
        base.Move(direction);
        _moveDirection = direction;
    }

    private void Jump() {
        if ( ! Entity.IsOnFloor() ) return;
        Entity.Inertia += Entity.UpDirection * 15f;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
        float floatDelta = (float)delta;



        float newSpeed = 0f;
        if ( ! _moveDirection.IsZeroApprox() ) {

            float directionLength = _moveDirection.Length();
            Vector3 normalizedDirection = _moveDirection / directionLength;

            // Vector3 groundedMovement = _moveDirection;
            // if (Entity.IsOnFloor()) {
            //     groundedMovement = Entity.UpDirection.FromToBasis(Entity.GetFloorNormal()) * groundedMovement;
            //     // Debug.DrawRay(Entity.transform.position, Entity.groundHit.normal, new Color(1,0,0,1));
            //     // Debug.DrawRay(Entity.transform.position, groundedMovement, Color.blue);
            // }

            Entity.AbsoluteForward = Entity.AbsoluteForward.SafeSlerp( normalizedDirection, Entity.Character.Data.rotationSpeed * floatDelta);
            

            // if ( isCurrentlyEvading ) {
            //     currentEvadeBehaviour.currentDirection = currentEvadeBehaviour.currentDirection.Lerp(Entity.AbsoluteForward, currentEvadeBehaviour.time * currentEvadeBehaviour.time);
            // } else {
                _rotationForward = normalizedDirection;
            // }

            /// Select the speed based on the movement type
            newSpeed = _movementSpeed switch {
                MovementSpeed.Walk => Entity.CharacterData.slowSpeed,
                MovementSpeed.Run => Entity.CharacterData.baseSpeed,
                MovementSpeed.Sprint => Entity.CharacterData.sprintSpeed,
                _ => newSpeed
            };

            Entity.Character.RotateTowards(Basis.LookingAt(_rotationForward, Entity.UpDirection), delta);
        }

        
        float speedDelta = Mathf.Abs(newSpeed - _moveSpeed) * (newSpeed != 0f ? 1f / newSpeed : 1f); // Accelerate faster depending on how big the difference between current and target speeds are
        speedDelta = Mathf.Clamp(_moveSpeed < newSpeed ? speedDelta : speedDelta * 0.5f, 0f, 1f); // Slow down faster than speeding up, clamped to 0-1
        _moveSpeed = Mathf.MoveToward(_moveSpeed, newSpeed, speedDelta * Entity.CharacterData.acceleration * floatDelta);

        Entity.Movement = _moveDirection * _moveSpeed;

	}
}
