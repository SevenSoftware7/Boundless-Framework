using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class EntityBehaviourManager : Node {

    [Export] public Entity Entity { 
        get => _entity;
        set => _entity ??= value;
    }
    private Entity _entity;
    [Export] public EntityBehaviour CurrentBehaviour { get; private set; }


    public EntityBehaviourManager() : base() {
        Name = nameof(EntityBehaviourManager);
    }
    public EntityBehaviourManager(Entity entity) : this() {
        Entity = entity;
    }
    

    public void SetBehaviour<TBehaviour>() where TBehaviour : EntityBehaviour, new() {
        CurrentBehaviour?.SetProcess(false);

        TBehaviour behaviour = GetNodeOrNull<TBehaviour>(typeof(TBehaviour).Name);
        behaviour ??= new() {
            Entity = Entity,
            BehaviourManager = this
        };

        behaviour.Start(CurrentBehaviour);
        CurrentBehaviour = behaviour;
        
        CurrentBehaviour.SetProcess(true);
    }
    
}
