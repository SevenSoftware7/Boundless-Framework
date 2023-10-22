using Godot;
using System;
using System.Diagnostics.CodeAnalysis;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class EntityBehaviourManager : Node {

    [Export] public Entity Entity { get; private set; }
    [Export] public EntityBehaviour? CurrentBehaviour { get; private set; }


    public EntityBehaviourManager() : base() {
        Entity ??= null !;

        Name = nameof(EntityBehaviourManager);
    }
    public EntityBehaviourManager(Entity entity) : this() {
        ArgumentNullException.ThrowIfNull(entity);

        Entity = entity;
    }

    

    public void SetBehaviour<TBehaviour>(TBehaviour? behaviour, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {

        if ( behaviour is not null && behaviour.GetParent() != this ) {
            throw new ArgumentException($"{nameof(behaviour)} does not belong to {nameof(EntityBehaviourManager)}.");
        }

        if ( behaviour is null && creator is not null ) {
            behaviour = creator.Invoke();
        }

        CurrentBehaviour?.SetProcess(false);
        if ( CurrentBehaviour?.FreeOnStop ?? false ) {
            CurrentBehaviour?.Free();
        }

        behaviour?.Start(CurrentBehaviour);
        CurrentBehaviour = behaviour;
        
        CurrentBehaviour?.SetProcess(true);
    }

    public void SetBehaviour<TBehaviour>(NodePath behaviourPath, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {

        TBehaviour? behaviour = GetNodeOrNull<TBehaviour>(behaviourPath);
        if ( behaviour is null && creator is null ) {
            throw new ArgumentException($"{nameof(behaviourPath)} not found in {nameof(EntityBehaviourManager)}.");
        }

        SetBehaviour(behaviour, creator);
    }

    public void SetBehaviour<TBehaviour>(Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {

        TBehaviour? behaviour = this.GetNodeByTypeName<TBehaviour>();
        if ( behaviour is null && creator is null ) {
            throw new ArgumentException($"{nameof(TBehaviour)} not found in {nameof(EntityBehaviourManager)}.");
        }

        SetBehaviour(behaviour, creator);
    }



#if TOOLS
    public override string[] _GetConfigurationWarnings() {
        string[] baseWarnings = base._GetConfigurationWarnings();

        if ( Engine.IsEditorHint() ) return baseWarnings;

        if ( Entity is null ) {
            baseWarnings ??= Array.Empty<string>();
            Array.Resize(ref baseWarnings, baseWarnings.Length + 1);
            baseWarnings[^1] = "Entity is null. EntityBehaviourManager requires an Entity.";
        }

        return baseWarnings;
    }
#endif


}
