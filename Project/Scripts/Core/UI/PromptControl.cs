using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class PromptControl : Control {
	[Export] public bool PromptVisible;
	[Export] private RichTextLabel Label = null!;
	[Export] private TextureRect Key = null!;

	private float velocity;


	public void SetText(string text) {
		Label.Text = text;
	}
	public void SetKey(Texture2D image) {
		Key.Texture = image;
	}

	public void Update(bool IsVisible, string text) {
		PromptVisible = IsVisible;

		if (PromptVisible) {
			SetText(text);
		}
	}
	public void Update(bool IsVisible, string text, Texture2D key) {
		PromptVisible = IsVisible;

		if (PromptVisible) {
			SetText(text);
			SetKey(key);
		}
	}
	public void Update(InteractTarget? interactTarget) {
		PromptVisible = interactTarget is not null;

		if (interactTarget is not null) {
			SetText(interactTarget.Interactable.InteractLabel);
		}
	}



	public override void _Process(double delta) {
		base._Process(delta);

		float floatDelta = (float)delta;

		Vector2 targetPosition = Position with {
			X = PromptVisible ? 0 : - Size.X
		};
		Position = Position.Lerp(targetPosition, 15f * floatDelta);

		Vector2 targetScale = Scale with {
			Y = ! PromptVisible /* && Position.IsEqualApprox(target) */ ? Mathf.Epsilon : 1f
		};
		Scale = Scale.Lerp(targetScale, 25f * floatDelta);


		bool wasNotVisible = ! Visible;
		Visible = ! Mathf.IsEqualApprox(Scale.Y, Mathf.Epsilon);
		if (Visible && wasNotVisible) {
			GetParent()?.MoveChild(this, 0);
		}

		UpdateMinimumSize();
	}

	public override Vector2 _GetMinimumSize() {
		return Size * Scale;
	}
}
