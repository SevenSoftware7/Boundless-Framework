using Godot;
using System;


namespace LandlessSkies.Core;

public sealed class EntityBehaviourManager {

	public Entity Entity { get; private set; } = null!;
	public EntityBehaviour? CurrentBehaviour { get; private set; }



	public EntityBehaviourManager(Entity entity) {
		ArgumentNullException.ThrowIfNull(entity);

		Entity = entity;
	}



	public void SetBehaviour<TBehaviour>(TBehaviour? behaviour, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {
		behaviour ??= creator?.Invoke();

		CurrentBehaviour?.SetProcess(false);
		if ( CurrentBehaviour is not null && CurrentBehaviour.FreeOnStop ) {
			CurrentBehaviour?.Free();
		}

		behaviour?.Start(CurrentBehaviour);
		CurrentBehaviour = behaviour;

		CurrentBehaviour?.SetProcess(true);
	}

	public void SetBehaviour<TBehaviour>(NodePath behaviourPath, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {

		TBehaviour? behaviour = Entity.GetNodeOrNull<TBehaviour>(behaviourPath);
		if ( behaviour is null && creator is null ) {
			throw new ArgumentException($"{nameof(behaviourPath)} not found in {nameof(EntityBehaviourManager)}.");
		}

		SetBehaviour(behaviour, creator);
	}

	public void SetBehaviour<TBehaviour>(Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {

		TBehaviour? behaviour = Entity.GetNodeByTypeName<TBehaviour>();
		if ( behaviour is null && creator is null ) {
			throw new ArgumentException($"{nameof(TBehaviour)} not found in {nameof(EntityBehaviourManager)}.");
		}

		SetBehaviour(behaviour, creator);
	}

}
