namespace LandlessSkies.Core;

public interface IInputReader {
	public void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice);
}