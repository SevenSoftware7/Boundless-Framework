namespace LandlessSkies.Core;

public interface IPlayerHandler {
	public virtual bool HasSetupPlayer => true;
	public void SetupPlayer(Player player);
	public void HandlePlayer(Player player);
	public void DisavowPlayer();
}