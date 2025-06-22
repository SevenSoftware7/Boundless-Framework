namespace SevenDev.Boundless;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;

public partial class TraitModifierRemover : TraitModifierOperation {
	public required TimeSpan Duration { init => _durationSec = value.TotalSeconds; }
	private double _durationSec = 0;

	public TimeSpan Delay { init => _delaySec = value.TotalSeconds; }
	private double _delaySec = 0;

	[AllowNull]
	public TraitModifierCollection.InterpFunction InterpolationFunction {
		get;
		init => field = value ?? Mathf.Lerp;
	} = Mathf.Lerp;
	private double _elapsedSec = 0f;


	public TraitModifierRemover(TraitModifierCollection collection, ITraitModifier modifier) : base(collection, modifier) { }

	public override void _Ready() {
		base._Ready();

		if (!ModifierCollection.Contains(Modifier)) {
			QueueFree();
			return;
		}

		if (_delaySec == 0 && _durationSec == 0) {
			ModifierCollection.Remove(Modifier);
			QueueFree();
			return;
		}
		ModifierCollection.SetMultiplier(Modifier, 1f);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		try {
			double t = (_elapsedSec - _delaySec) / _durationSec;
			t = Mathf.Clamp(t, 0, 1);
			ModifierCollection.SetMultiplier(Modifier, (float)InterpolationFunction(1, 0, t));
		}
		catch (KeyNotFoundException) {
			QueueFree();
			return;
		}

		if (_elapsedSec >= _delaySec + _durationSec) {
			ModifierCollection.Remove(Modifier);
			QueueFree();
			return;
		}

		_elapsedSec += delta;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		if (what != NotificationPredelete) return;


		ModifierCollection.Remove(Modifier);
	}

}