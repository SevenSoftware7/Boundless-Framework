namespace LandlessSkies.Core;

public interface IInputHandler {
	public void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice, HudManager hud);
}