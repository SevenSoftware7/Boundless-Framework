using Godot;

namespace LandlessSkies.Core;

public interface IEnablable {
	bool IsEnabled { get; set; }

	sealed void SetEnabled(bool enabled) {
		IsEnabled = enabled;
	}

	public sealed void Enable() {
		if (IsEnabled)
			return;

		EnableBehaviour();
	}
	public sealed void Disable() {
		if (!IsEnabled)
			return;

		DisableBehaviour();
	}

	public sealed void EnableDisable(bool enabled) {
		if (enabled) {
			Enable();
		}
		else {
			Disable();
		}
	}

	protected void EnableBehaviour() { }
	protected void DisableBehaviour() { }
}