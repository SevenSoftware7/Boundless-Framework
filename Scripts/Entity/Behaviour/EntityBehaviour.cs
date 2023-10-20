using Godot;
using System;


namespace LandlessSkies.Core;

[GlobalClass]
public abstract partial class EntityBehaviour : Node {

    [Export] public Entity Entity { get; set; }

    [Export] public EntityBehaviourManager BehaviourManager { 
        get => _behaviourManager;
        set {
            _behaviourManager = value;
            GetParent()?.RemoveChild(this);
            _behaviourManager.AddChildSetOwner(this);
        }
    }
    private EntityBehaviourManager _behaviourManager;



    public EntityBehaviour() : base() {}


    public virtual void HandleInput(Player.InputInfo inputInfo) {;}

    public virtual void SetSpeed(MovementSpeed speed) {;}
    public virtual void Move(Vector3 direction) {;}

    public virtual void Start(EntityBehaviour previousBehaviour) {;}

    public enum MovementSpeed {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Sprint = 3
    }
}
