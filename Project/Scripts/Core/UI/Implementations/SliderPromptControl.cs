namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class SliderPromptControl : PromptControl {
	[Export] private bool shrinkInView;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Key = null!;

	private float velocity;
	private bool _queuedForDestruction = false;

	// Store the position and scale here because some precision is lost when modifying the Position and Scale properties
	private float _positionX;
	private float _scaleY;


	public override void SetText(string text) {
		Label.Text = text;
	}
	public override void SetKey(Texture2D image) {
		Key.Texture = image;
	}

	public override void _Ready() {
		base._Ready();
		Enabled = false;
		Position = Position with { X = -Size.X };
		Scale = new(1f, 0f);
		Visible = false;

		_positionX = Position.X;
		_scaleY = Scale.Y;
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


		float targetPositionX = (Enabled && !_queuedForDestruction) ? 0 : -Size.X;
		_positionX = _positionX.ClampedLerp(targetPositionX, 15f * floatDelta);
		Position = Position with { X = _positionX };

		float targetScaleY = !(Enabled && !_queuedForDestruction) && (shrinkInView || _positionX.IsEqualApprox(targetPositionX)) ? 0f : 1f;
		_scaleY = _scaleY.ClampedLerp(targetScaleY, 25f * floatDelta);
		Scale = Scale with { Y = _scaleY };


		bool wasNotVisible = !Visible;
		Visible = !_scaleY.IsZeroApprox();
		if (Visible && wasNotVisible) {
			GetParent()?.MoveChild(this, 0);
		}

		UpdateMinimumSize();
	}

	public override Vector2 _GetMinimumSize() {
		return Size * Scale;
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