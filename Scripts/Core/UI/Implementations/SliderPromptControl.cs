namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class SliderPromptControl : PromptControl {
	[Export] private bool shrinkInView;
	[Export] private Control Wrapper = null!;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Key = null!;

	private float velocity;
	private bool _queuedForDestruction = false;


	public override void SetText(string text) {
		Label.Text = text;
	}
	public override void SetKey(Texture2D image) {
		Key.Texture = image;
	}

	public override void _Ready() {
		base._Ready();
		Enabled = false;
		Visible = false;

		Size = Size with { Y = 0f };
		CustomMinimumSize = CustomMinimumSize with { Y = 0f };

		Wrapper.Position = Wrapper.Position with { X = -Wrapper.Size.X };
	}

	public override void Destroy() {
		base.Destroy();
		if (Visible) {
			_queuedForDestruction = true;
		}
		else {
			QueueFree();
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (_queuedForDestruction && !Visible) {
			QueueFree();
			return;
		}

		float floatDelta = (float)delta;

		Vector2 size = Wrapper.Size;
		Vector2 position = Wrapper.Position;


		float targetPositionX = !(Enabled && !_queuedForDestruction) ? -size.X : 0f;
		float targetSizeY = !(Enabled && !_queuedForDestruction) && (shrinkInView || Mathf.Abs(position.X - targetPositionX) <= 0.25f) ? 0f : size.Y;

		float sizeY = Size.Y.ClampedLerp(targetSizeY, 25f * floatDelta);
		CustomMinimumSize = CustomMinimumSize with { Y = sizeY };
		Size = Size with { Y = sizeY };

		Wrapper.Position = position with { X = position.X.ClampedLerp(targetPositionX, 15f * floatDelta) };


		bool wasNotVisible = !Visible;
		Visible = !(sizeY <= 0.25f);
		if (Visible && wasNotVisible) {
			GetParent()?.MoveChild(this, 0);
		}

	}

	// public override void _Notification(int what) {
	// 	base._Notification(what);
	// 	switch((ulong)what) {
	// 		case NotificationPredelete:
	// 			if (Visible) {
	// 				_queuedForDestruction = true;
	// 				CancelFree();
	// 			}
	// 			break;
	// 	}
	// }
}