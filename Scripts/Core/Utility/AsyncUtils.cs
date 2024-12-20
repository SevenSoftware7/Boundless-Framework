namespace LandlessSkies.Core;

using System;
using System.Threading.Tasks;
using Godot;

public static class AsyncUtils {
	public static async Task Wait(uint timeMilliseconds) {
		float elapsed = 0;
		int increment = 1000 / Engine.PhysicsTicksPerSecond;
		while (elapsed < timeMilliseconds) {
			await Task.Delay(increment);
			elapsed += increment;
		}
	}
	public static async Task WaitAndCall(uint timeMilliseconds, Action<int> action) {
		int elapsed = 0;
		int increment = 1000 / Engine.PhysicsTicksPerSecond;
		while (elapsed < timeMilliseconds) {
			await Task.Delay(increment);
			elapsed = Math.Min(elapsed + increment, (int)timeMilliseconds);
			action(elapsed);
		}
	}
}