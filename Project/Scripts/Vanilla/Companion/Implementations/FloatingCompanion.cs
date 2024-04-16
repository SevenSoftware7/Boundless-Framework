namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;
using SevenGame.Utility;

[Tool]
[GlobalClass]
public partial class FloatingCompanion : Companion {
	public bool OnFace { get; private set; }
	public bool OnRight { get; private set; } = true;
	public Transform3D Subject { get; private set; } = Transform3D.Identity;
	public Transform3D Head { get; private set; } = Transform3D.Identity;
	[Export] public float T { get; private set; }
	[Export] public float TFace { get; private set; }

	public Vector3 HoveringPosition { get; private set; }
	[Export(PropertyHint.Layers3DPhysics)] private uint CollisionMask = uint.MaxValue;

	[Export] public Curve3D Curve { get; private set; }


	public FloatingCompanion() : base() {
		Curve = CreateCurve();
	}
	public FloatingCompanion(CompanionData data, CompanionCostume costume) : base(data, costume) {
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
	public override void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice) {
		base.HandleInput(entity, cameraController, inputDevice);

		Callable.From(() => {
			Subject = entity.Character?.GlobalTransform ?? entity.GlobalTransform;

			if (entity.Skeleton is not null && entity.Skeleton.TryGetBoneTransform("Head", out var boneTransform)) {
				Head = boneTransform;
			}
		}).CallDeferred();

		OnFace |= inputDevice.IsActionPressed("focus");
	}



	private bool PositionBlocked(Vector3 position) {
		SphereShape3D sphere = new() {
			Radius = (CompanionModel?.GetAabb().GetLongestAxisSize() ?? 1f) + 0.5f,
		};

		return GetWorld3D().CollideShape3D(Transform3D.Identity.Translated(position), out _, sphere, CollisionMask, maxResults: 1);
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (Engine.IsEditorHint()) {
			return;
		}

		if (Subject == Transform3D.Identity) {
			return;
		}

		Callable.From(() => {
			float floatDelta = (float)delta;
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

			float GetCurveT() {
				return OnRight ? 1f : 0f;
			}

			Vector3 GetPosition(float t) {
				return Head.Origin + Head.Basis * Curve.Sample(0, t);
			}
		}).CallDeferred();
	}
}