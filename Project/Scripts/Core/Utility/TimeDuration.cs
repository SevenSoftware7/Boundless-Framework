namespace SevenGame.Utility;

using Godot;

public struct TimeDuration {
	public ulong DurationMsec { get; init; } = 0;
	public ulong StopTime { get; private set; } = 0;

	public readonly bool IsDone => Time.GetTicksMsec() >= StopTime;
	public readonly ulong RemainingDuration => IsDone ? 0 : StopTime - Time.GetTicksMsec();
	public readonly ulong Overtime => IsDone ? Time.GetTicksMsec() - StopTime : 0;



	public TimeDuration(ulong durationMsec) {
		DurationMsec = durationMsec;
		Start();
	}



	public void End() {
		StopTime = Time.GetTicksMsec();
	}
	public void Start() {
		StopTime = Time.GetTicksMsec() + DurationMsec;
	}


	public static implicit operator float(TimeDuration timeUntil) => timeUntil.RemainingDuration;
}