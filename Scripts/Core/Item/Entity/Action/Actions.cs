namespace LandlessSkies.Core;

public static class Actions {
	public static bool CanCancel(this Action? action) => action is null || action.IsCancellable;
}