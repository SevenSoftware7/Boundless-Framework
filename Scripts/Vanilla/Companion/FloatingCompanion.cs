namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class FloatingCompanion : Companion, IPlayerHandler {
	private static Vector3 rightPosition = new(1.2f, 0.3f, 0.2f);
	private static Vector3 leftPosition = rightPosition * new Vector3(-1f, 1f, 1f);


	public bool OnFace { get; private set; }
	public bool OnRight { get; private set; } = true;
	public Transform3D Subject { get; private set; } = Transform3D.Identity;
	public Transform3D Head { get; private set; } = Transform3D.Identity;

	public Vector3 HoveringPosition { get; private set; }
	public Basis HoveringRotation { get; private set; } = Basis.Identity;


	public Entity? Entity { get; private set; }

	[Export] public float T { get; private set; }
	[Export] public float TFace { get; private set; }

	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = CollisionLayers.Terrain | CollisionLayers.Water;

	[Export] public Curve3D Curve { get; private set; }


	public FloatingCompanion() : base() {
		Curve = new() {
			UpVectorEnabled = false
		};

		Curve.AddPoint(Vector3.Zero);
		Curve.AddPoint(Vector3.Zero);
	}


	private bool PositionBlocked(Vector3 position) {
		SphereShape3D sphere = new() {
			Radius = (CostumeHolder?.Costume?.GetAabb().GetLongestAxisSize() ?? 1f) + 0.5f,
		};

		return GetWorld3D().CollideShape3D(Transform3D.Identity.Translated(position), out _, sphere, CollisionMask, maxResults: 1, collideWithAreas: false);
	}


	public void HandlePlayer(Player player) {
		OnFace |= player.InputDevice.IsActionPressed(Inputs.Focus) || player.Entity?.CurrentBehaviour is SwimmingBehaviour;
	}
	public void DisavowPlayer() { }


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is null) return;

		float floatDelta = (float)delta;

		Subject = Entity.GlobalTransform;

		if (Entity.Skeleton is not null && Entity.Skeleton.TryGetBoneTransform("Head", out Transform3D boneTransform)) {
			Head = boneTransform.RotatedLocal(Vector3.Up, Mathf.DegToRad(180f));
		}

		// FIXME: On-Faceness should not be directly linked to the healing ability
		if (OnFace && Entity?.Health is not null) {
			Entity.Health.Value += floatDelta * 10f;
		}

		bool rightBlocked = PositionBlocked(Head * rightPosition);
		bool leftBlocked = PositionBlocked(Head * leftPosition);

		OnFace |= rightBlocked && leftBlocked;

		if (!OnFace && (OnRight ? rightBlocked : leftBlocked))
			OnRight = !OnRight;


		T = T.MoveToward(OnRight ? 1f : 0f, 6f * floatDelta);
		TFace = TFace.Lerp(OnFace ? 1f : 0f, (OnFace ? 18f : 6f) * floatDelta);

		Vector3 tPosition = Head * leftPosition.Lerp(rightPosition, T);

		HoveringPosition = HoveringPosition.Lerp(tPosition, 8.5f * floatDelta);
		HoveringRotation = HoveringRotation.SafeSlerp(Subject.Basis, 12f * floatDelta);


		float LeftRightT = T * 2f - 1f;
		float distance = tPosition.DistanceSquaredTo(Head.Origin);

		Curve.SetPointPosition(0, HoveringPosition);
		Curve.SetPointOut(0, Head.Basis * new Vector3(0, 0, -0.1f));

		Curve.SetPointIn(1, Head.Basis * new Vector3(LeftRightT * distance * 0.3f, 0, -0.2f));
		Curve.SetPointPosition(1, Head * new Vector3(0, 0.25f, -0.15f));

		GlobalTransform = GlobalTransform with {
			Origin = Curve.Sample(0, TFace),
			Basis = HoveringRotation.Slerp(Head.Basis, TFace),
		};

		OnFace = false;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationParented:
				Entity ??= GetParent() as Entity;
				break;
		}
	}
}