namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class AnimationAttack(Entity entity, SingleWeapon weapon, StringName library) : Attack(entity, weapon) {
	public override bool IsCancellable => false;
	public override bool IsKnockable => true;

	protected abstract StringName AnimationName { get; }


	private void OnChanged(StringName oldName, StringName newName) {
		Stop();
	}
	private void OnFinished(StringName name) {
		Stop();
	}

	protected override void _Start() {
		if (Entity?.AnimationPlayer is not null) {
			Entity.AnimationPlayer.Stop();

			try {
				Entity.AnimationPlayer.Play(GetAnimationPath(library, AnimationName));
				Entity.AnimationPlayer.AnimationChanged += OnChanged;
				Entity.AnimationPlayer.AnimationFinished += OnFinished;
			} catch (Exception) {
				Stop();
			}
		}
	}
	protected override void _Stop() {
		if (Entity?.AnimationPlayer is not null && Entity.AnimationPlayer.CurrentAnimation == GetAnimationPath(library, AnimationName)) {
			Entity.AnimationPlayer.Stop();
		}
	}
}