using Godot;
using System;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterData : Resource {
    
    [Export] public CharacterCostume? BaseCostume { get; private set; }
    [Export] public PackedScene? CollisionScene { get; private set; }
    [Export] public PackedScene? SkeletonScene { get; private set; }
    
    public const float DEFAULT_MAX_HEALTH = 100f;
    [Export] public float maxHealth = DEFAULT_MAX_HEALTH;

    public const float DEFAULT_STEP_HEIGHT = 0.5f;
    [Export] public float stepHeight = DEFAULT_STEP_HEIGHT;

    public const float DEFAULT_WEIGHT = 12f;
    [Export] public float weight = DEFAULT_WEIGHT;

    public const float DEFAULT_JUMP_HEIGHT = 16f;
    [Export] public float jumpHeight = DEFAULT_JUMP_HEIGHT;

    public const float DEFAULT_ACCELERATION = 100f;
    [Export] public float acceleration = DEFAULT_ACCELERATION;
    
    public const float DEFAULT_ROTATION_SPEED = 100f;
    [Export] public float rotationSpeed = DEFAULT_ROTATION_SPEED;

    public const float DEFAULT_BASE_SPEED = 8f;
    [Export] public float baseSpeed = DEFAULT_BASE_SPEED;

    public const float DEFAULT_SPRINT_SPEED = 13f;
    [Export] public float sprintSpeed = DEFAULT_SPRINT_SPEED;

    public const float DEFAULT_SLOW_SPEED = 5f;
    [Export] public float slowSpeed = DEFAULT_SLOW_SPEED;

    public const float DEFAULT_SWIM_SPEED = 4.25f;
    [Export] public float swimMultiplier = DEFAULT_SWIM_SPEED;

    public const float DEFAULT_EVADE_SPEED = 27f;
    [Export] public float evadeSpeed = DEFAULT_EVADE_SPEED;

    public const float DEFAULT_EVADE_DURATION = 0.6f;
    [Export] public float evadeDuration = DEFAULT_EVADE_DURATION;

    public const float DEFAULT_EVADE_COOLDOWN = 0.6f;
    [Export] public float evadeCooldown = DEFAULT_EVADE_COOLDOWN;

    public float TotalEvadeDuration => evadeDuration + evadeCooldown;



    public virtual Character Instantiate(Node3D root, CharacterCostume? costume = null) {
        return new Character(this, costume ?? BaseCostume, root);
    }
}
