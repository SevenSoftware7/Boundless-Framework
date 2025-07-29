namespace Seven.Boundless;

using Godot;

public static class PlayerHandlerExtensions {
	public static void PropagatePlayerHandling(this Node parent, Player player) {
		foreach (Node child in parent.GetChildren()) {
			child.PropagatePlayerHandling(player);
		}

		if (parent is IPlayerHandler handlerParent) {
			handlerParent.HandlePlayer(player);
		}
	}
	public static void PropagatePlayerDisavowing(this Node parent) {
		foreach (Node child in parent.GetChildren()) {
			child.PropagatePlayerDisavowing();
		}

		if (parent is IPlayerHandler handlerParent) {
			handlerParent.DisavowPlayer();
		}
	}
}