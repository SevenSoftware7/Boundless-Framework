namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class SliderPromptControl : PromptControl {
	private const float NEAR_ZERO = 1E-2F;
	[Export] private bool shrinkInView;

	[Export] private Control Wrapper = null!;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Key = null!;

	private bool _queuedForDestruction = false;
	private float _slideProgress = 1.0f;
	private float _shrinkProgress = 1.0f;

	private Vector2 _minimumSize;
	public override Vector2 _GetMinimumSize() => _minimumSize;


	public override void SetText(string text) {
		Label.Text = text;
	}
	public override void SetKey(Texture2D image) {
		Key.Texture = image;
	}

	public override void _Ready() {
		base._Ready();
		Enabled = false;

		UpdateSlideState();
	}

	public override void Destroy() {
		base.Destroy();
		if (_queuedForDestruction) return;

		if (Visible) {
			_queuedForDestruction = true;
		}
		else {
			QueueFree();
		}
	}

	private void UpdateSlideState() {
		Vector2 size = Wrapper.Size;
		Vector2 position = GlobalPosition;


		Vector2 hidPosition = direction switch {
			PromptHideDirection.Left => new Vector2(-size.X, position.Y),
			PromptHideDirection.Top => new Vector2(position.X, -size.Y),
			PromptHideDirection.Right => new Vector2(GetViewportRect().Size.X + size.X, position.Y),
			PromptHideDirection.Bottom => new Vector2(position.X, GetViewportRect().Size.Y + size.Y),
			_ => Vector2.Zero
		};

		Vector2 targetPosition = position.Lerp(hidPosition, _slideProgress);
		Wrapper.GlobalPosition = targetPosition;

		Vector2 oldMinimumSize = _minimumSize;
		switch (direction) {
			case PromptHideDirection.Left:
			case PromptHideDirection.Right:
				float targetYSize = size.Y.Lerp(0f, _shrinkProgress);

				_minimumSize = size with { Y = targetYSize };
				Size = Size with { Y = targetYSize };
				break;
			case PromptHideDirection.Top:
			case PromptHideDirection.Bottom:
				float targetXSize = size.X.Lerp(0f, _shrinkProgress);

				_minimumSize = size with { X = targetXSize };
				Size = Size with { X = targetXSize };
				break;
		}
		if (oldMinimumSize != _minimumSize) {
			UpdateMinimumSize();
		}


		bool wasNotVisible = !Visible;
		bool isVisible = true;
		Visible = !(Mathf.Abs(_shrinkProgress - 1f) < NEAR_ZERO);
		if (isVisible && wasNotVisible) {
			GetParent()?.MoveChild(this, 0);
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (_queuedForDestruction && !Visible) {
			QueueFree();
			return;
		}

		float floatDelta = (float)delta;

		bool isDeployed = Enabled && !_queuedForDestruction;
		bool shouldShrink = !isDeployed && (shrinkInView || Mathf.Abs(_slideProgress - 1f) < NEAR_ZERO);

		_slideProgress = _slideProgress.ClampedLerp(isDeployed ? 0f : 1f, floatDelta * 15f);
		_shrinkProgress = _shrinkProgress.ClampedLerp(shouldShrink ? 1f : 0f, floatDelta * 25f);

		UpdateSlideState();
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