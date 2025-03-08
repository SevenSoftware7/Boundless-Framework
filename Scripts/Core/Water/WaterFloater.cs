namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public sealed partial class WaterFloater : Node, IWaterDisplacementSubscriber, IWaterCollisionListener {
	[Export] public RigidBody3D? Body;
	[Export] public Node3D? Origin;
	[Export] public uint floaterCount = 1;

	private Water? Water;
	private float waterSurfaceDisplacement = 0f;
	private bool ShouldExitWater = false;

	private const float waterLinearDamp = 2.5f;
	private const float waterAngularDamp = 1f;

	private (float linearDamp, float angularDamp, bool canSleep)? oldConfig;

	public (Vector3, WaterMesh)? GetInfo() => Body is null || Water?.Mesh is null ? null : (Body.GlobalPosition, Water.Mesh);
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => waterSurfaceDisplacement = waterDisplacement.Y;

	private void ExitWater() {
		ShouldExitWater = false;

		Water = null;

		if (Body is not null) {
			if (Body.LinearDamp == waterLinearDamp) Body.LinearDamp = oldConfig?.linearDamp ?? 0;
			if (Body.AngularDamp == waterAngularDamp) Body.AngularDamp = oldConfig?.angularDamp ?? 0;
			if (!Body.CanSleep) Body.CanSleep = oldConfig?.canSleep ?? true;
		}
		oldConfig = null;
	}

	public void OnEnterWater(Water water) {
		if (water == Water && oldConfig is not null) {
			ShouldExitWater = false;
			return;
		}

		Water = water;

		if (Body is not null && oldConfig is null) {
			oldConfig = (Body.LinearDamp, Body.AngularDamp, Body.CanSleep);

			Body.LinearDamp = waterLinearDamp;
			Body.AngularDamp = waterAngularDamp;
			Body.CanSleep = false;
		}
	}

	public void OnExitWater(Water water) {
		if (water != Water) return;

		ShouldExitWater = true;
	}



	public override void _EnterTree() {
		base._EnterTree();
		WaterDisplacementEffect.Subscribers.Add(this);
	}

	public override void _ExitTree() {
		base._ExitTree();
		WaterDisplacementEffect.Subscribers.Remove(this);
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what){
			case NotificationPredelete:
				WaterDisplacementEffect.Subscribers.Remove(this);
				break;
		}
	}

	public override void _PhysicsProcess(double delta) {
		base._PhysicsProcess(delta);
		if (Body is null || Water is null) return;

		Vector3 position = Origin?.GlobalPosition ?? Body.GlobalPosition;

		float waterSurface = Water.GetSurfaceInDirection(position, Vector3.Up, out Collisions.IntersectRay3DResult res)
			? res.Point.Y + waterSurfaceDisplacement
			: Mathf.Inf;

		if (ShouldExitWater && (position.Y > waterSurface + 1f || waterSurface == Mathf.Inf)) {
			ExitWater();
			return;
		}

		const float floatability = 1f;
		Vector3 offset = Origin is null ? Vector3.Zero : Origin.GlobalPosition - Body.GlobalPosition;
		float distanceToWaterSurface = waterSurface - position.Y;
		float displacementMultiplier = Mathf.Clamp(distanceToWaterSurface * floatability, -floatability, floatability) + 1f;

		Body.ApplyForce(new Vector3(0f, Mathf.Abs(Body.GetGravity().Y * Body.Mass) * displacementMultiplier / floaterCount, 0f), offset);
	}
}