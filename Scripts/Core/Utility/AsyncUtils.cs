namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public static class AsyncUtils {
	private static readonly int increment = 1000 / Engine.PhysicsTicksPerSecond;
	public static async Task Wait(uint timeMilliseconds) {
		int elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay(increment);
			elapsed += increment;
		}
	}
	public static async Task WaitAndCall(uint timeMilliseconds, Action<int> action) {
		int elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay(increment);
			elapsed = Math.Min(elapsed + increment, (int)timeMilliseconds);
			action(elapsed);
		}
	}


	public static async IAsyncEnumerable<int> WaitAndYield(uint timeMilliseconds) {
		int elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay(increment);
			elapsed = Math.Min(elapsed + increment, (int)timeMilliseconds);
			yield return elapsed;
		}
	}
}