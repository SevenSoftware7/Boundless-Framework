namespace LandlessSkies.Core;

public partial class Entity {
	public float CharacterMaxHealth => Character?.Data.maxHealth ?? CharacterData.DEFAULT_MAX_HEALTH;
	public float CharacterStepHeight => Character?.Data.stepHeight ?? CharacterData.DEFAULT_STEP_HEIGHT;
	public float CharacterWeight => Character?.Data.weight ?? CharacterData.DEFAULT_WEIGHT;
	public float CharacterJumpHeight => Character?.Data.jumpHeight ?? CharacterData.DEFAULT_JUMP_HEIGHT;
	public float CharacterAcceleration => Character?.Data.acceleration ?? CharacterData.DEFAULT_ACCELERATION;
	public float CharacterRotationSpeed => Character?.Data.rotationSpeed ?? CharacterData.DEFAULT_ROTATION_SPEED;
	public float CharacterBaseSpeed => Character?.Data.baseSpeed ?? CharacterData.DEFAULT_BASE_SPEED;
	public float CharacterSprintSpeed => Character?.Data.sprintSpeed ?? CharacterData.DEFAULT_SPRINT_SPEED;
	public float CharacterSlowSpeed => Character?.Data.slowSpeed ?? CharacterData.DEFAULT_SLOW_SPEED;
}