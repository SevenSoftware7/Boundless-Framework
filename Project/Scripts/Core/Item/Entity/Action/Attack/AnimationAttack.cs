namespace LandlessSkies.Core;

using System;
using Godot;

public abstract partial class AnimationAttack(Entity entity, Weapon weapon, StringName library) : Attack(entity, weapon) {
	private bool _isCancellable = false;

	public override bool IsCancellable => _isCancellable;
	public override bool IsKnockable => true;

	protected abstract StringName AnimationName { get; }


	public void SetCancellable(bool cancellable) {
		_isCancellable = cancellable;
		_SetCancellable(cancellable);
	}
	protected virtual void _SetCancellable(bool cancellable) { }

	private void OnStarted(StringName name) {
		GD.PrintS("Started", name);
		Stop();
	}
	private void OnChanged(StringName oldName, StringName newName) {
		GD.PrintS("Changed", oldName, newName);
		Stop();
	}
	private void OnFinished(StringName name) {
		GD.PrintS("Finished", name);
		Stop();
	}


	protected override void _Start() {
		GD.Print("Start");
		if (Entity?.AnimationPlayer is null) {
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
		GD.Print("Stop");
		if (Entity?.AnimationPlayer is null) return;

		if (Entity.AnimationPlayer.CurrentAnimation == GetAnimationPath(library, AnimationName)) {
			Entity.AnimationPlayer.Stop();
		}
	}
}