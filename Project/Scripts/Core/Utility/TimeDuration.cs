namespace SevenDev.Utility;

using Godot;

/// <summary>
/// TimeDuration is used to check whether a specific time duration has passed or not
/// </summary>
public class TimeDuration {
	public ulong DurationMsec;

	public ulong StopTime { get; private set; } = Time.GetTicksMsec();

	public bool IsDone => Time.GetTicksMsec() >= StopTime;
	public ulong TimeLeft => IsDone ? 0 : StopTime - Time.GetTicksMsec();
	public ulong Overtime => IsDone ? Time.GetTicksMsec() - StopTime : 0;


	public TimeDuration(ulong durationMsec = 1000, bool start = false) {
		DurationMsec = durationMsec;
		if (start) Start();
	}


	public void End() {
		StopTime = Time.GetTicksMsec();
	}
	public void Start() {
		StopTime = Time.GetTicksMsec() + DurationMsec;
	}


	public static implicit operator float(TimeDuration timeUntil) => timeUntil.TimeLeft;
	public static implicit operator bool(TimeDuration timeUntil) => timeUntil.IsDone;
}