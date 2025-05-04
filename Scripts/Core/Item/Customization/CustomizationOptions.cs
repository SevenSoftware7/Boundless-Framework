namespace LandlessSkies.Core;

using System;
using Godot;

public sealed class CustomizationOptions : ICustomization {
	public readonly StringName Name;

	private readonly OptionState[] Options;
	private readonly Action<OptionState> OnUpdate;

	public ICustomizationState State {
		get => _state;
		set {
			_state = value switch {
				OptionState state => state,
				_ => Options[0]
			};
			OnUpdate?.Invoke(_state);
		}
	}
	private OptionState _state;

	public CustomizationOptions(StringName name, OptionState[] options, Action<OptionState> onUpdate) {
		Name = name;
		Options = options;
		OnUpdate = onUpdate;

		_state = options[0];
	}



	public readonly struct OptionState(StringName Value, StringName Name) : ICustomizationState {
		public readonly StringName Name = Name;
		public readonly StringName Value = Value;
	}
}