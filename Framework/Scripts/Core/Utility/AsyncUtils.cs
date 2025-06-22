namespace SevenDev.Boundless;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncUtils {
	public static async Task Wait(uint timeMilliseconds, float incrementMs = 16) {
		float elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay((int)incrementMs);
			elapsed += incrementMs;
		}
	}
	public static async Task WaitAndCall(uint timeMilliseconds, Action<float> action, float incrementMs = 16) {
		float elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay((int)incrementMs);
			elapsed = Math.Min(elapsed + incrementMs, (int)timeMilliseconds);
			action(elapsed);
		}
	}


	public static async IAsyncEnumerable<float> WaitAndYield(uint timeMilliseconds, float incrementMs = 16) {
		float elapsed = 0;
		while (elapsed < timeMilliseconds) {
			await Task.Delay((int)incrementMs);
			elapsed = Math.Min(elapsed + incrementMs, (int)timeMilliseconds);
			yield return elapsed;
		}
	}
}