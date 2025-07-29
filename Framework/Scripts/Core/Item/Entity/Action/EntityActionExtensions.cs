namespace Seven.Boundless;

using System.Runtime.CompilerServices;

public static class EntityActionExtensions {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanCancel(this EntityAction? action) => action is null || action.IsCancellable;
}