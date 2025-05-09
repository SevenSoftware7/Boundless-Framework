namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;

public partial class TraitModifierAdder : TraitModifierOperation {
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


	public TraitModifierAdder(TraitModifierCollection collection, ITraitModifier modifier) : base(collection, modifier) { }

	public override void _Ready() {
		base._Ready();

		if (ModifierCollection.Contains(Modifier)) {
			QueueFree();
			return;
		}
		ModifierCollection.Add(Modifier);

		if (_delaySec == 0 && _durationSec == 0) {
			QueueFree();
			return;
		}
		ModifierCollection.SetMultiplier(Modifier, 0f);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		try {
			double t = (_elapsedSec - _delaySec) / _durationSec;
			t = Mathf.Clamp(t, 0, 1);
			ModifierCollection.SetMultiplier(Modifier, (float)InterpolationFunction(0, 1, t));
		}
		catch (KeyNotFoundException) {
			QueueFree();
			return;
		}

		if (_elapsedSec >= _delaySec + _durationSec) {
			QueueFree();
			return;
		}

		_elapsedSec += delta;
	}
}