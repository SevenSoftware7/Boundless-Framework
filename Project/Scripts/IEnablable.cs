

namespace LandlessSkies.Core;

public interface IEnablable {
	sealed void SetEnabled(bool enabled) {
		if (enabled) {
			Enable();
		} else {
			Disable();
		}
	}
	void Enable();
	void Disable();
}