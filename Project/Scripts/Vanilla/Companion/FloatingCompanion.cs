namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Utility;

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

	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = Collisions.Terrain | Collisions.Water;

	[Export] public Curve3D Curve { get; private set; }


	private FloatingCompanion() : base() {
		Curve = new() {
			UpVectorEnabled = false
		};

		Curve.AddPoint(Vector3.Zero);
		Curve.AddPoint(Vector3.Zero);
	}


	private bool PositionBlocked(Vector3 position) {
		SphereShape3D sphere = new() {
			Radius = (CostumeHolder?.Model?.GetAabb().GetLongestAxisSize() ?? 1f) + 0.5f,
		};

		return GetWorld3D().CollideShape3D(Transform3D.Identity.Translated(position), out _, sphere, CollisionMask, maxResults: 1, collideWithAreas: false);
	}


	public void HandlePlayer(Player player) {
		OnFace |= player.InputDevice.IsActionPressed(Inputs.Focus);
	}
	public void DisavowPlayer() { }


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is null) return;

		float floatDelta = (float)delta;

		Subject = Entity.GlobalTransform;

		if (Entity.Skeleton is not null && Entity.Skeleton.TryGetBoneTransform("Head", out Transform3D boneTransform)) {
			Head = boneTransform.RotatedLocal(Vector3.Up, Mathfs.Deg2Rad(180f));
		}

		if (OnFace && Entity?.Health is not null) {
			Entity.Health.Value += floatDelta;
		}

		OnFace |= PositionBlocked(Head * rightPosition) && PositionBlocked(Head * leftPosition);

		if (!OnFace && PositionBlocked(GetPosition(GetCurveT())))
			OnRight = !OnRight;


		T = Mathf.MoveToward(T, GetCurveT(), 6f * floatDelta);
		TFace = Mathf.Lerp(TFace, OnFace ? 1f : 0f, 12f * floatDelta);

		Vector3 tPosition = GetPosition(T);

		HoveringPosition = HoveringPosition.Lerp(tPosition, 8.5f * floatDelta);
		HoveringRotation = HoveringRotation.SafeSlerp(Subject.Basis, 12f * floatDelta);


		// Curve to Face


		float LeftRightT = T * 2f - 1f;
		float distance = HoveringPosition.DistanceSquaredTo(Head.Origin);

		Curve.SetPointPosition(0, HoveringPosition);
		Curve.SetPointOut(0, Head.Basis * new Vector3(0, 0, -0.1f));

		Curve.SetPointIn(1, Head.Basis * new Vector3(LeftRightT * distance * 0.3f, 0, -0.2f));
		Curve.SetPointPosition(1, Head * new Vector3(0, 0.25f, -0.15f));

		GlobalTransform = GlobalTransform with {
			Origin = Curve.Sample(0, TFace),
			Basis = HoveringRotation.Slerp(Head.Basis, TFace),
		};

		OnFace = false;

		float GetCurveT() => OnRight ? 1f : 0f;
		Vector3 GetPosition(float t) => Head * leftPosition.Lerp(rightPosition, t);
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