using Godot;

namespace SevenGame.Utility;

public struct TimeLimit {
	public ulong stopTime = 0;

	public readonly bool IsDone => Time.GetTicksMsec() >= stopTime;
	public readonly ulong RemainingDuration => IsDone ? 0 : stopTime - Time.GetTicksMsec();



	public TimeLimit() {;}



	public void End() {
		stopTime = Time.GetTicksMsec();
	}
	public void SetTime(ulong timeMsec){
		stopTime = timeMsec;
	}
	public void SetDurationMSec(ulong durationMsec){
		stopTime = Time.GetTicksMsec() + durationMsec;
	}


	public static implicit operator float(TimeLimit timeUntil) => timeUntil.RemainingDuration;
}