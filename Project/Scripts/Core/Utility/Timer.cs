namespace SevenDev.Utility;

using Godot;

/// <summary>
/// Timer is used to measure how much time has passed since it was started
/// </summary>
public class Timer {
	public ulong startTime = 0;
	public ulong Duration => Time.GetTicksMsec() - startTime;



	public Timer(bool start = true) {
		if (start) Start();
	}



	public void Start() {
		startTime = Time.GetTicksMsec();
	}


	public static implicit operator ulong(Timer timer) => timer.Duration;
}