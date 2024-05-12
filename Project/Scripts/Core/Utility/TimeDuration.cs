namespace SevenDev.Utility;

using Godot;

/// <summary>
/// TimeDuration is used to check whether a specific time duration has passed or not
/// </summary>
public struct TimeDuration(ulong durationMsec = 1000) {
	public ulong DurationMsec = durationMsec;
	public ulong StopTime { get; private set; } = 0;

	public readonly bool IsDone => Time.GetTicksMsec() >= StopTime;
	public readonly ulong RemainingDuration => IsDone ? 0 : StopTime - Time.GetTicksMsec();
	public readonly ulong Overtime => IsDone ? Time.GetTicksMsec() - StopTime : 0;


	public void End() {
		StopTime = Time.GetTicksMsec();
	}
	public void Start() {
		StopTime = Time.GetTicksMsec() + DurationMsec;
	}


	public static implicit operator float(TimeDuration timeUntil) => timeUntil.RemainingDuration;
	public static implicit operator bool(TimeDuration timeUntil) => timeUntil.IsDone;
}