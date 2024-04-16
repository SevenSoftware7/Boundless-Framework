namespace LandlessSkies.Core;

using Godot;
using System;

public sealed class EntityBehaviourManager {
	public Entity Entity { get; private set; } = null!;
	public EntityBehaviour? CurrentBehaviour { get; private set; }



	public EntityBehaviourManager(Entity entity) {
		ArgumentNullException.ThrowIfNull(entity);

		Entity = entity;
	}



	public void SetBehaviour<TBehaviour>(TBehaviour? behaviour) where TBehaviour : EntityBehaviour {
		behaviour?.Start(CurrentBehaviour);
		CurrentBehaviour?.Stop();

		CurrentBehaviour = behaviour;
	}

	public void SetBehaviour<TBehaviour>(Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {
		SetBehaviour(typeof(TBehaviour).Name, creator);
	}

	public void SetBehaviour<TBehaviour>(NodePath behaviourPath, Func<TBehaviour>? creator = null) where TBehaviour : EntityBehaviour {
		TBehaviour? behaviour = Entity.GetNodeOrNull<TBehaviour>(behaviourPath);
		if (behaviour is null && creator is null) {
			throw new ArgumentException($"{nameof(behaviourPath)} not found in {nameof(EntityBehaviourManager)}.");
		}

		SetBehaviour(creator?.Invoke());
	}
}