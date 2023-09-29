using Godot;
using System;

namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterData : Resource {
    
    [Export] public CharacterCostume BaseCostume { get; private set; }
    [Export] public PackedScene CollisionScene { get; private set; }
    [Export] public PackedScene SkeletonScene { get; private set; }
    
    [Export] public float maxHealth = 100f;
    [Export] public Vector3 size = new(0.5f, 2f, 0.5f);
    [Export] public float stepHeight = 0.5f;
    [Export] public float weight = 12f;
    [Export] public float jumpHeight = 16f;

    [Export] public float baseSpeed = 8f;
    [Export] public float sprintSpeed = 13f;
    [Export] public float slowSpeed = 5f;
    [Export] public float acceleration = 100f;
    [Export] public float swimMultiplier = 0.85f;

    [Export] public float evadeSpeed = 27f;
    [Export] public float evadeDuration = 0.6f;
    [Export] public float evadeCooldown = 0.01f;

    public float TotalEvadeDuration => evadeDuration + evadeCooldown;


    public CharacterData() : base() {;}


    public virtual Character CreateCollisions(IModelAttachment modelAttachment) {
        return new Character(modelAttachment, this);
    }
}
