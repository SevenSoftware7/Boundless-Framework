namespace SevenDev.Utility;

using Godot;

/// <summary>
/// TimeDuration is used to check whether a specific time duration has passed or not
/// </summary>
public class TimeDuration {
	public ulong DurationMsec;

	public ulong StopTime { get; private set; }

	public bool HasPassed => Time.GetTicksMsec() >= StopTime;
	public ulong TimeLeft => HasPassed ? 0 : StopTime - Time.GetTicksMsec();
	public ulong Overtime => HasPassed ? Time.GetTicksMsec() - StopTime : 0;

	public TimeDuration(bool start, ulong durationMsec = 1000) {
		DurationMsec = durationMsec;
		if (start) Start();
	}


	public void End() {
		StopTime = Time.GetTicksMsec();
	}
	public void Start() {
		StopTime = Time.GetTicksMsec() + DurationMsec;
	}


	public static implicit operator float(TimeDuration timeDuration) => timeDuration.TimeLeft;
	public static implicit operator bool(TimeDuration timeDuration) => timeDuration.HasPassed;
}