namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Utility;


/// <summary>
/// An Attack that mainly functions as an Animation, with callback events.
/// </summary>
/// <param name="entity">Inherited from <see cref="Action"/>.</param>
/// <param name="path">The AnimationPath used to play the corresponding animation.</param>
/// <param name="modifiers">Inherited from <see cref="Action"/>.</param>
public abstract partial class AnimationAction(Entity entity, AnimationPath path, IEnumerable<AttributeModifier>? modifiers = null) : Action(entity, modifiers) {
	public override bool IsCancellable => _isCancellable;
	private bool _isCancellable = false;

	public override bool IsInterruptable => _isInterruptable;
	private bool _isInterruptable = false;

	private AnimationPlayer AnimPlayer = entity.AnimationPlayer is AnimationPlayer animPlayer
		? animPlayer
		: throw new InvalidOperationException($"Could not initialize AnimationAction, because no AnimationPlayer could be found");
	protected AnimationPath AnimationPath = path;





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


	private void OnAnimationStarted(StringName path) {
		if (path != AnimationPath) Stop();
	}
	private void OnAnimationChanged(StringName oldName, StringName newName) {
		Stop();
	}
	private void OnAnimationFinished(StringName name) {
		Stop();
	}


	protected override void _Start() {
		GD.Print($"Starting animation: {AnimationPath}");
		AnimPlayer.Stop();
		try {
			AnimPlayer.Play(AnimationPath);
			AnimPlayer.AnimationStarted += OnAnimationStarted;
			AnimPlayer.AnimationChanged += OnAnimationChanged;
			AnimPlayer.AnimationFinished += OnAnimationFinished;
		}
		catch (Exception e) {
			GD.PushError($"{GetType().Name} AnimationAction error: {e}");
			Stop();
		}
	}
	protected override void _Stop() {
		if (AnimPlayer.CurrentAnimation == AnimationPath) {
			AnimPlayer.Stop();
		}
	}
}