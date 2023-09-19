using Godot;
using System;


namespace EndlessSkies.Core;

[GlobalClass]
public abstract partial class EntityBehaviour : Node {

    [Export] public Entity Entity { get; set; }

    [Export] public EntityBehaviourManager BehaviourManager { 
        get => _behaviourManager;
        set {
            _behaviourManager = value;
            GetParent()?.RemoveChild(this);
            _behaviourManager.AddChild(this);
            Owner = _behaviourManager.Owner;
        }
    }
    private EntityBehaviourManager _behaviourManager;

    public EntityBehaviour() : base() {}


    public abstract void Start(EntityBehaviour previousBehaviour);
}
