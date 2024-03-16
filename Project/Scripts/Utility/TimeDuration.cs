namespace SevenGame.Utility;

using Godot;

public struct TimeDuration(ulong durationMsec) {
	public ulong stopTime = 0;

	public readonly bool IsDone => Time.GetTicksMsec() >= stopTime;
	public readonly ulong RemainingDuration => IsDone ? 0 : stopTime - Time.GetTicksMsec();



	public void End() {
		stopTime = Time.GetTicksMsec();
	}
	public void Start() {
		stopTime = Time.GetTicksMsec() + durationMsec;
	}


	public static implicit operator float(TimeDuration timeUntil) => timeUntil.RemainingDuration;
}