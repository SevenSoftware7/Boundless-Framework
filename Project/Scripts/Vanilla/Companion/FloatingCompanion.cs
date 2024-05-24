namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class FloatingCompanion : Companion {
	public bool OnFace { get; private set; }
	public bool OnRight { get; private set; } = true;
	public Transform3D Subject { get; private set; } = Transform3D.Identity;
	public Transform3D Head { get; private set; } = Transform3D.Identity;

	[Export] public Entity? Entity { get; private set; }

	[Export] public float T { get; private set; }
	[Export] public float TFace { get; private set; }

	public Vector3 HoveringPosition { get; private set; }
	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue;

	[Export] public Curve3D Curve { get; private set; }


	private FloatingCompanion() : base() {
		Curve = CreateCurve();
	}
	public FloatingCompanion(CompanionCostume costume) : base(costume) {
		Curve = CreateCurve();
	}



	private static Curve3D CreateCurve() {
		Vector3 rightOffset = new(1.2f, 0.3f, 0.2f);
		Vector3 LeftOffset = new(-1.2f, 0.3f, 0.2f);

		Curve3D curve = new() {
			UpVectorEnabled = false
		};
		curve.AddPoint(LeftOffset, @out: Vector3.Forward);
		// curve.AddPoint(Vector3.Zero, Vector3.Forward, Vector3.Forward);
		curve.AddPoint(rightOffset, @in: Vector3.Forward);

		return curve;
	}
	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		OnFace |= player.InputDevice.IsActionPressed("focus");
	}



	private bool PositionBlocked(Vector3 position) {
		SphereShape3D sphere = new() {
			Radius = (Model?.GetAabb().GetLongestAxisSize() ?? 1f) + 0.5f,
		};

		return GetWorld3D().CollideShape3D(Transform3D.Identity.Translated(position), out _, sphere, CollisionMask, maxResults: 1, collideWithAreas: false);
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) return;
		if (Entity is null) return;

		float floatDelta = (float)delta;

		if (Entity is not null) {
			Subject = Entity.GlobalTransform;

			if (Entity.Skeleton is not null && Entity.Skeleton.TryGetBoneTransform("Head", out Transform3D boneTransform)) {
				Head = boneTransform;
			}
		}

		if (OnFace && Entity?.Health is not null) {
			Entity.Health.Amount += floatDelta;
		}

		OnFace |= PositionBlocked(GetPosition(0f)) && PositionBlocked(GetPosition(1f));

		if (!OnFace && PositionBlocked(GetPosition(GetCurveT())))
			OnRight = !OnRight;


		T = Mathf.MoveToward(T, OnFace ? 0.5f : GetCurveT(), 6f * floatDelta);
		TFace = Mathf.MoveToward(TFace, OnFace ? 1f : 0f, 8f * floatDelta);


		HoveringPosition = HoveringPosition.Lerp(GetPosition(T), 10f * floatDelta);
		Vector3 finalPosition = HoveringPosition.Lerp(Head.Origin, TFace);
		Basis finalRotation = Subject.Basis.SafeSlerp(Head.Basis, TFace);


		GlobalTransform = GlobalTransform with {
			Origin = finalPosition,
			Basis = finalRotation,
		};

		OnFace = false;

		float GetCurveT() => OnRight ? 1f : 0f;
		Vector3 GetPosition(float t) => Head.Origin + Head.Basis * Curve.Sample(0, t);
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