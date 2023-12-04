using Godot;
using System;
using System.Diagnostics.CodeAnalysis;


namespace LandlessSkies.Core;

[GlobalClass]
public abstract partial class EntityBehaviour : Node {

    public abstract bool FreeOnStop { get; }

    [Export] public Entity Entity { get; set; }
    [Export] public EntityBehaviourManager BehaviourManager;



    public EntityBehaviour() : base() {
        Entity ??= null !;
        BehaviourManager ??= null !;
    }
    public EntityBehaviour([MaybeNull] Entity entity, [MaybeNull] EntityBehaviourManager behaviourManager) : base() {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(behaviourManager);

        Entity = entity;
        BehaviourManager = behaviourManager;
        BehaviourManager.AddChildAndSetOwner(this);
    }



    public virtual void HandleInput(Player.InputInfo inputInfo) {}

    public virtual void SetSpeed(MovementSpeed speed) {}
    public virtual void Move(Vector3 direction) {}

    public virtual void Start(EntityBehaviour? previousBehaviour) {}



    public enum MovementSpeed {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Sprint = 3
    }
}
