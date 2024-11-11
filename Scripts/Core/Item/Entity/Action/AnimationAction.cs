namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;

/// <summary>
/// An Attack that mainly functions through Animation, with callback events.
/// </summary>
/// <param name="entity">Inherited from <see cref="Action"/>.</param>
/// <param name="path">The AnimationPath used to play the corresponding animation.</param>
/// <param name="innateModifiers">Inherited from <see cref="Action"/>.</param>
public abstract partial class AnimationAction(Entity entity, AnimationPath path, IEnumerable<AttributeModifier>? innateModifiers = null) : Action(entity, innateModifiers) {
	private readonly List<AttributeModifier?> modifiers = [];
	public override bool IsCancellable => _isCancellable;
	private bool _isCancellable = false;

	public override bool IsInterruptable => _isInterruptable;
	private bool _isInterruptable = false;

	private Vector3 _movement = Vector3.Zero;
	private MovementBehaviour.MovementType _movementType = MovementBehaviour.MovementType.Run;

	private AnimationPlayer AnimPlayer = entity.AnimationPlayer is AnimationPlayer animPlayer
		? animPlayer
		: throw new InvalidOperationException($"Could not initialize AnimationAction, because no AnimationPlayer could be found");
	protected readonly AnimationPath AnimationPath = path;


	public void SetMovement(Vector3 direction, MovementBehaviour.MovementType movementType) {
		_movement = direction;
		_movementType = movementType;
	}

	/// <summary>
	/// Method to add an Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="modifier">The Attribute Modifier to add</param>
	public void AddAttributeModifier(AttributeModifier modifier) {
		modifier = (AttributeModifier)modifier.Duplicate();
		if (modifier is null) return;

		modifiers.Add(modifier);
		Entity.AttributeModifiers.Add(modifier);
	}
	/// <summary>
	/// Method to add an Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="modifier">The Attribute Modifier to add</param>
	/// <param name="timeMilliseconds">The time in milliseconds the AttributeModifier will take to "fade in"</param>
	public async void AddAttributeModifier(AttributeModifier modifier, uint timeMilliseconds = 0) {
		modifier = (AttributeModifier)modifier.Duplicate();
		if (modifier is null) return;

		modifiers.Add(modifier);
		await Entity.AttributeModifiers.AddProgressively(modifier, timeMilliseconds);
	}

	/// <summary>
	/// Method to remove an already existing Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="index">The Cache Index of the Attribute Modifier to remove</param>
	public void RemoveAttributeModifier(int index) {
		if (index < 0 || modifiers.Count <= index) return;

		AttributeModifier? modifier = modifiers[index];
		if (modifier is null) return;

		modifiers[index] = null;
		Entity.AttributeModifiers.Remove(modifier);
	}
	/// <summary>
	/// Method to remove an already existing Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="index">The Cache Index of the Attribute Modifier to remove</param>
	/// <param name="timeMilliseconds">The time in milliseconds the AttributeModifier will take to "fade out"</param>
	public async void RemoveAttributeModifier(int index, uint timeMilliseconds = 0) {
		if (index < 0 || modifiers.Count <= index) return;

		AttributeModifier? modifier = modifiers[index];
		if (modifier is null) return;

		modifiers[index] = null;
		await Entity.AttributeModifiers.RemoveProgressively(modifier, timeMilliseconds);
	}
	/// <summary>
	/// Method to remove an already existing Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="modifier">The Attribute Modifier to remove</param>
	/// <param name="timeMilliseconds">The time in milliseconds the AttributeModifier will take to "fade out"</param>
	public void RemoveAttributeModifier(AttributeModifier modifier) {
		if (modifier is null) return;

		int index = modifiers.IndexOf(modifier);
		if (index < 0) return;

		modifiers[index] = null;
		Entity.AttributeModifiers.Remove(modifier);
	}
	/// <summary>
	/// Method to remove an already existing Attribute Modifier.<para/>
	/// Mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="modifier">The Attribute Modifier to remove</param>
	public async void RemoveAttributeModifier(AttributeModifier modifier, uint timeMilliseconds = 0) {
		if (modifier is null) return;

		int index = modifiers.IndexOf(modifier);
		if (index < 0) return;

		modifiers[index] = null;
		await Entity.AttributeModifiers.RemoveProgressively(modifier, timeMilliseconds);
	}

	/// <summary>
	/// Method to enable or disable the Attack Cancellability.<para/>
	/// mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="cancellable">Whether the Attack should be cancellable</param>
	public void SetCancellable(bool cancellable) {
		_isCancellable = cancellable;
		_SetCancellable(cancellable);
	}
	/// <summary>
	/// Callback method for when the Cancellability of the Attack is updated.
	/// </summary>
	/// <param name="cancellable">Whether the attack was set to be cancellable or not</param>
	protected virtual void _SetCancellable(bool cancellable) { }

	/// <summary>
	/// Method to enable or disable the Attack Interruptability.<para/>
	/// mainly for Animations, but can be used via script too.
	/// </summary>
	/// <param name="interruptable">Whether the Attack should be interruptable</param>
	public void SetInterruptable(bool interruptable) {
		_isInterruptable = interruptable;
		_SetInterruptable(interruptable);
	}
	/// <summary>
	/// Callback method for when the Interruptability of the Attack is updated.
	/// </summary>
	/// <param name="interruptable">Whether the attack was set to be interruptable or not</param>
	protected virtual void _SetInterruptable(bool interruptable) { }


	private void OnAnimationStarted(StringName path) {
		if (path != AnimationPath) Stop();
	}
	private void OnAnimationChanged(StringName oldName, StringName newName) {
		if (oldName == AnimationPath) {
			Callable.From(() => {
				_AnimationChanged(newName);
				Stop();
			}).CallDeferred();
		}
	}
	protected virtual void _AnimationChanged(StringName newName) { }

	private void OnAnimationFinished(StringName name) {
		if (name == AnimationPath) {
			Callable.From(() => {
				_AnimationFinished();
				Stop();
			}).CallDeferred();
		}
	}
	protected virtual void _AnimationFinished() { }


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

		AnimPlayer.AnimationStarted -= OnAnimationStarted;
		AnimPlayer.AnimationChanged -= OnAnimationChanged;
		AnimPlayer.AnimationFinished -= OnAnimationFinished;

		Entity.AttributeModifiers.RemoveRange(modifiers.OfType<AttributeModifier>());
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (!_movement.IsZeroApprox() && Entity.CurrentBehaviour is GroundedBehaviour groundedBehaviour) {
			groundedBehaviour.Move(Basis.LookingAt(Entity.GlobalForward, Entity.UpDirection) * _movement, _movementType);
		}
	}
}