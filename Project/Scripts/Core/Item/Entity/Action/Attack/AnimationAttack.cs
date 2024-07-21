namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;

public abstract partial class AnimationAttack(Entity entity, Weapon weapon, StringName library, IEnumerable<AttributeModifier>? modifiers = null) : Attack(entity, weapon, modifiers) {

	public override bool IsCancellable => _isCancellable;
	private bool _isCancellable = false;

	public override bool IsInterruptable => true;
	private bool _isInterruptable = false;

	protected abstract StringName AnimationName { get; }


	public void SetCancellable(bool cancellable) {
		_isCancellable = cancellable;
		_SetCancellable(cancellable);
	}
	protected virtual void _SetCancellable(bool cancellable) { }
	public void SetInterruptable(bool interruptable) {
		_isInterruptable = interruptable;
		_SetInterruptable(interruptable);
	}
	protected virtual void _SetInterruptable(bool interruptable) { }


	private void OnStarted(StringName name) {
		Stop();
	}
	private void OnChanged(StringName oldName, StringName newName) {
		Stop();
	}
	private void OnFinished(StringName name) {
		Stop();
	}


	protected override void _Start() {
		if (Entity.AnimationPlayer is null) {
			GD.PushError($"Could not start {GetType().Name} AnimationAttack, because the no AnimationPlayer could be found");
			Stop();
			return;
		}

		Entity.AnimationPlayer.Stop();
		try {
			Entity.AnimationPlayer.Play(GetAnimationPath(library, AnimationName));
			// Entity.AnimationPlayer.AnimationStarted += OnStarted;
			Entity.AnimationPlayer.AnimationChanged += OnChanged;
			Entity.AnimationPlayer.AnimationFinished += OnFinished;
		} catch (Exception e) {
			GD.Print(e);
			Stop();
		}
	}
	protected override void _Stop() {
		if (Entity?.AnimationPlayer is null) return;

		if (Entity.AnimationPlayer.CurrentAnimation == GetAnimationPath(library, AnimationName)) {
			Entity.AnimationPlayer.Stop();
		}
	}
}