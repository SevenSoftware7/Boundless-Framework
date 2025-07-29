namespace Seven.Boundless;

/// <summary>
/// Interface that is used to allow a class to Handle a Player.<para/>
/// For example: react to the Player's Inputs or setup UI.<para/>
/// </summary>
public interface IPlayerHandler {
	/// <summary>
	/// Handle Player behaviour such as Device Input, UI and Camera Control.
	/// </summary>
	/// <param name="player">The Player to Handle</param>
	public void HandlePlayer(Player player);

	/// <summary>
	/// Stop Handling the Player.<para/>
	/// Reset all the UI and Inputs here.<para/>
	/// </summary>
	public void DisavowPlayer();
}