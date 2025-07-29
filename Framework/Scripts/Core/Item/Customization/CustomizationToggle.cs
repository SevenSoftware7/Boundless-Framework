namespace Seven.Boundless;

using System;

public struct CustomizationToggle : ICustomization {
	public readonly string Name;
	private readonly Action<bool> OnUpdate;

	public ICustomizationState State {
		readonly get => _state;
		set {
			_state = value switch {
				ToggleState state => state,
				_ => ToggleState.False
			};
			OnUpdate?.Invoke(_state.Value);
		}
	}
	private ToggleState _state = ToggleState.False;


	public CustomizationToggle(string name, Action<bool> onUpdate) {
		Name = name;
		OnUpdate = onUpdate;
	}


	public readonly struct ToggleState : ICustomizationState {
		public static readonly ToggleState True = new(true);
		public static readonly ToggleState False = new(false);

		public readonly bool Value;

		private ToggleState(bool Value) {
			this.Value = Value;
		}

	}
}