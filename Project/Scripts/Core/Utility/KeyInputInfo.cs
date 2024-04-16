namespace SevenGame.Utility;

public struct KeyInputInfo {
	private const float HOLD_TIME = 0.15f;


	public bool currentValue = false;
	public bool lastValue = false;

	public Timer trueTimer = new();
	public Timer falseTimer = new();

	public readonly bool Started => currentValue && !lastValue;
	public readonly bool Stopped => !currentValue && lastValue;



	public KeyInputInfo() { }



	public static bool SimultaneousTap(KeyInputInfo a, KeyInputInfo b, float time = HOLD_TIME) {
		bool aTapped = a.trueTimer.Duration < time && b.Started;
		bool bTapped = b.trueTimer.Duration < time && a.Started;
		return aTapped || bTapped;
	}

	public readonly bool Tapped(float time = HOLD_TIME) => Stopped && trueTimer.Duration < time;
	public readonly bool Held(float time = HOLD_TIME) => currentValue && trueTimer.Duration > time;

	public readonly bool SimultaneousTap(KeyInputInfo other, float time = HOLD_TIME) {
		return SimultaneousTap(this, other, time);
	}

	public void SetVal(bool value) {
		currentValue = value;

		if (currentValue) {
			falseTimer.Start();
		}
		else {
			trueTimer.Start();
		}
	}


	public static implicit operator bool(KeyInputInfo data) => data.currentValue;
}