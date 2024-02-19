using Godot;
using System;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterData : Resource, IUIObject {
	public const float DEFAULT_MAX_HEALTH = 100f;
	public const float DEFAULT_STEP_HEIGHT = 0.5f;
	public const float DEFAULT_WEIGHT = 12f;
	public const float DEFAULT_JUMP_HEIGHT = 16f;
	public const float DEFAULT_ACCELERATION = 100f;
	public const float DEFAULT_ROTATION_SPEED = 100f;
	public const float DEFAULT_BASE_SPEED = 8f;
	public const float DEFAULT_SPRINT_SPEED = 13f;
	public const float DEFAULT_SLOW_SPEED = 5f;
	
	

	[Export] public CharacterCostume? BaseCostume { get; private set; }
	[Export] public PackedScene? CollisionScene { get; private set; }
	[Export] public PackedScene? SkeletonScene { get; private set; }
	
	[Export] public string DisplayName { get; private set; } = "";
	public Texture2D? DisplayPortrait => BaseCostume?.DisplayPortrait;
	
	[Export] public float maxHealth = DEFAULT_MAX_HEALTH;
	[Export] public float stepHeight = DEFAULT_STEP_HEIGHT;
	[Export] public float weight = DEFAULT_WEIGHT;
	[Export] public float jumpHeight = DEFAULT_JUMP_HEIGHT;
	[Export] public float acceleration = DEFAULT_ACCELERATION;
	[Export] public float rotationSpeed = DEFAULT_ROTATION_SPEED;
	[Export] public float baseSpeed = DEFAULT_BASE_SPEED;
	[Export] public float sprintSpeed = DEFAULT_SPRINT_SPEED;
	[Export] public float slowSpeed = DEFAULT_SLOW_SPEED;


	public virtual Character Instantiate(CharacterCostume? costume = null) =>
		new(this, costume ?? BaseCostume);
}
