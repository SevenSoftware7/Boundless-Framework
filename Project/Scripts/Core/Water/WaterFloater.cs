namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[GlobalClass]
public sealed partial class WaterFloater : Node, IWaterDisplacementSubscriber, IWaterCollisionNotifier {
	[Export] public RigidBody3D? Body;
	[Export] public Node3D? Origin;
	[Export] public Water? Water;
	[Export] public uint floaterCount = 1;
	private float waterSurfaceDisplacement = 0f;
	private const float waterLinearDamp = 2.5f;
	private const float waterAngularDamp = 1f;

	private (float linearDamp, float angularDamp, bool canSleep)? oldConfig;

	public void Enter(Water water) {
		if (Body is not null) {
			oldConfig = (Body.LinearDamp, Body.AngularDamp, Body.CanSleep);

			Body.LinearDamp = waterLinearDamp;
			Body.AngularDamp = waterAngularDamp;
			Body.CanSleep = false;
		}
		Water = water;
	}

	public void Exit(Water water) {
		if (water != Water) return;

		if (Body is not null) {
			if (Body.LinearDamp == waterLinearDamp) Body.LinearDamp = oldConfig?.linearDamp ?? 0;
			if (Body.AngularDamp == waterAngularDamp) Body.AngularDamp = oldConfig?.angularDamp ?? 0;
			if (!Body.CanSleep) Body.CanSleep = oldConfig?.canSleep ?? true;
		}
		Water = null;
	}

	public (Vector3, WaterMesh)? GetInfo() => Body is null || Water?.Mesh is null ? null : (Body.GlobalPosition, Water.Mesh);
	public void UpdateWaterDisplacement(Vector3 waterDisplacement) => waterSurfaceDisplacement = waterDisplacement.Y;



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

		Vector3 position = Body.GlobalPosition;

		float waterSurface = Water.GetSurfaceInDirection(position, Vector3.Up, out Collisions.IntersectRay3DResult res)
			? res.Point.Y
			: Mathf.Inf;

		float totalWaterHeight = waterSurface + waterSurfaceDisplacement;


		if (position.Y > totalWaterHeight) return;

		const float floatability = 1f;
		Vector3 offset = Origin is null ? Vector3.Zero : Origin.GlobalPosition - position;
		float distanceToWaterSurface = totalWaterHeight - position.Y;
		float displacementMultiplier = Mathf.Clamp(distanceToWaterSurface * floatability, -floatability, floatability) + 1f;

		Body.ApplyForce(new Vector3(0f, Mathf.Abs(Body.GetGravity().Y) * displacementMultiplier, 0f), offset);
	}
}