using Godot;
using System;


namespace EndlessSkies.Core;

public partial class TestBehaviour : EntityBehaviour {

    private Vector3 moveDirection;
    private float moveSpeed;
    private MovementSpeed movementSpeed;

    public override void Start(EntityBehaviour previousBehaviour) {
        Entity.MotionMode = CharacterBody3D.MotionModeEnum.Grounded;
    }

    public override void HandleInput(Player.InputInfo inputInfo) {
        base.HandleInput(inputInfo);
        inputInfo.RawInputToGroundedMovement(camRotation: out _, out Vector3 groundedMovement);

        float speedSquared = groundedMovement.LengthSquared();
        if (speedSquared <= 0f)
            SetSpeed(MovementSpeed.Idle);
        // else if ( SingletonHelper.GetInstance<DialogueController>().Enabled || ((speedSquared <= 0.25 || controller.walkInput) && onGround) ) 
        //     SetSpeed(MovementSpeed.Slow);
        else if ( inputInfo.ControlDevice.GetSprintKey().currentValue )
            SetSpeed(MovementSpeed.Sprint);
        else
            SetSpeed(MovementSpeed.Run);

        Move(groundedMovement);
    }

    public override void SetSpeed(MovementSpeed speed) {
        base.SetSpeed(speed);
        
        bool isBusyEvading = /* Entity.Behaviour.EvadeBehaviour?.state ??  */false;
        if (Entity.IsOnFloor() && !isBusyEvading) {
            if (speed == MovementSpeed.Idle) {

                // MovementStopAnimation();
            
            } else if ((int)speed > (int)movementSpeed) {

                // MovementStartAnimation(speed);

            }
        }
        movementSpeed = speed;
    }

    public override void Move(Vector3 direction) {
        base.Move(direction);
        moveDirection = direction;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
        // Entity.Transform = Entity.Transform.Rotated(Vector3.Up, (float)delta * 0.5f);
        // Entity.Transform = Entity.Transform.Translated(moveDirection * (float)delta * 5);

        float floatDelta = (float)delta;



        float newSpeed = 0f;
        if ( moveDirection.LengthSquared() != 0f ) {

            // Vector3 groundedMovement = moveDirection;
            // if (Entity.groundDetected) {
            //     // groundedMovement = Entity.UpDirection.FromToRotation(Entity.groundHit.normal) * groundedMovement;
            //     // Debug.DrawRay(Entity.transform.position, Entity.groundHit.normal, new Color(1,0,0,1));
            //     // Debug.DrawRay(Entity.transform.position, groundedMovement, Color.blue);
            // }

            // Entity.AbsoluteForward = Entity.AbsoluteForward.Slerp( groundedMovement, 100f * floatDelta);

            // if ( isCurrentlyEvading ) {
            //     currentEvadeBehaviour.currentDirection = currentEvadeBehaviour.currentDirection.Lerp(Entity.AbsoluteForward, currentEvadeBehaviour.time * currentEvadeBehaviour.time);
            // } else {
            //     _rotationForward = moveDirection.Normalized();
            // }

            /// Select the speed based on the movement type
            switch (movementSpeed) {
                case MovementSpeed.Walk:
                    newSpeed = Entity.CharacterData.slowSpeed;
                    break;
                case MovementSpeed.Run:
                    newSpeed = Entity.CharacterData.baseSpeed;
                    break;
                case MovementSpeed.Sprint:
                    newSpeed = Entity.CharacterData.sprintSpeed;
                    break;
            }
        }
        
        // Entity.CharacterInstance.CharacterModel.RotateTowards(_rotationForward, Entity.UpDirection, delta);

        
        float speedDelta = Mathf.Abs(newSpeed - moveSpeed) * (newSpeed != 0f ? 1f / newSpeed : 1f); // Accelerate faster depending on how big the difference between current and target speeds are
        speedDelta = Mathf.Clamp(moveSpeed < newSpeed ? speedDelta : speedDelta * 0.5f, 0f, 1f); // Slow down faster than speeding up, clamped to 0-1
        moveSpeed = Mathf.MoveToward(moveSpeed, newSpeed, speedDelta * Entity.CharacterData.acceleration * floatDelta);



        Entity.Velocity = moveDirection * moveSpeed;
	}
}
