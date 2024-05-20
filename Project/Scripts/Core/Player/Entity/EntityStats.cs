namespace LandlessSkies.Core;

using Godot;


[Tool]
[GlobalClass]
public partial class EntityStats : Resource {
	public static readonly EntityStats DefaultStats = new();

	public const float DEFAULT_MAX_HEALTH = 1f;
	public const float DEFAULT_STEP_HEIGHT = 0.5f;
	public const float DEFAULT_WEIGHT = 12f;
	public const float DEFAULT_JUMP_HEIGHT = 15f;
	public const float DEFAULT_ACCELERATION = 50f;
	public const float DEFAULT_DECELERATION = 35f;
	public const float DEFAULT_ROTATION_SPEED = 20f;
	public const float DEFAULT_BASE_SPEED = 8f;
	public const float DEFAULT_SPRINT_SPEED = 14f;
	public const float DEFAULT_SLOW_SPEED = 3f;

	public EntityStats() : base() { }

	[Export] public float MaxHealth = DEFAULT_MAX_HEALTH;
	[Export] public float StepHeight = DEFAULT_STEP_HEIGHT;
	[Export] public float Weight = DEFAULT_WEIGHT;
	[Export] public float JumpHeight = DEFAULT_JUMP_HEIGHT;
	[Export] public float Acceleration = DEFAULT_ACCELERATION;
	[Export] public float Deceleration = DEFAULT_DECELERATION;
	[Export] public float RotationSpeed = DEFAULT_ROTATION_SPEED;
	[Export] public float BaseSpeed = DEFAULT_BASE_SPEED;
	[Export] public float SprintSpeed = DEFAULT_SPRINT_SPEED;
	[Export] public float SlowSpeed = DEFAULT_SLOW_SPEED;
}