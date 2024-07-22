namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// An Attack that mainly functions as an Animation, with callback events.
/// </summary>
/// <param name="entity">Inherited from <see cref="EntityAction"/>.</param>
/// <param name="weapon">Inherited from <see cref="Attack"/>.</param>
/// <param name="library">The Animation library used to play the corresponding animation.</param>
/// <param name="modifiers">Inherited from <see cref="EntityAction"/>.</param>
public abstract partial class AnimationAttack(Entity entity, Weapon weapon, StringName library, IEnumerable<AttributeModifier>? modifiers = null) : Attack(entity, weapon, modifiers) {

	public override bool IsCancellable => _isCancellable;
	private bool _isCancellable = false;

	public override bool IsInterruptable => _isInterruptable;
	private bool _isInterruptable = false;

	protected abstract StringName AnimationName { get; }


	/// <summary>
	/// Method to enable or disable the Attack Cancellability, mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="cancellable">Whether the Attack should be cancellable</param>
	public void SetCancellable(bool cancellable) {
		_isCancellable = cancellable;
		_SetCancellable(cancellable);
	}
	/// <summary>
	/// Callback method when updating the Cancellability of the Attack
	/// </summary>
	/// <param name="cancellable">Whether the attack was set to be cancellable or not</param>
	protected virtual void _SetCancellable(bool cancellable) { }

	/// <summary>
	/// Method to enable or disable the Attack Interruptability, mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="interruptable">Whether the Attack should be interruptable</param>
	public void SetInterruptable(bool interruptable) {
		_isInterruptable = interruptable;
		_SetInterruptable(interruptable);
	}
	/// <summary>
	/// Callback method when updating the Interruptability of the Attack
	/// </summary>
	/// <param name="interruptable">Whether the attack was set to be interruptable or not</param>
	protected virtual void _SetInterruptable(bool interruptable) { }


	private void OnStarted(StringName name) {
		if (name != GetAnimationPath(library, AnimationName)) Stop();
	}
	private void OnChanged(StringName oldName, StringName newName) {
		Stop();
	}
	private void OnFinished(StringName name) {
		Stop();
	}


	protected override void _Start() {
		if (Entity.AnimationPlayer is null) {
			GD.PushError($"Could not start {GetType().Name} AnimationAttack, because no AnimationPlayer could be found");
			Stop();
			return;
		}

		Entity.AnimationPlayer.Stop();
		try {
			Entity.AnimationPlayer.Play(GetAnimationPath(library, AnimationName));
			Entity.AnimationPlayer.AnimationStarted += OnStarted;
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