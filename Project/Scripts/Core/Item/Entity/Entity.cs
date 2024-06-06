namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class Entity : CharacterBody3D, IPlayerHandler, ICostumable<EntityCostume>, ICustomizable, ISaveable<Entity>, IInjectionBlocker<Skeleton3D?> {
	private readonly List<Vector3> standableSurfaceBuffer = [];
	private const int STANDABLE_SURFACE_BUFFER_SIZE = 20;
	private GaugeControl? healthBar;

	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public virtual List<ICustomization> GetCustomizations() => [];
	public virtual List<ICustomizable> GetSubCustomizables() => [.. GetChildren().OfType<ICustomizable>()];


	[Export] public EntityStats Stats { get; private set; } = new();
	[Export] public HudPack HudPack { get; private set; } = new();

	[Export] public virtual Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;
			this.PropagateInject(_handedness);
		}
	}
	private Handedness _handedness = Handedness.Right;


	[ExportGroup("Costume")]
	[Export] public EntityCostume? Costume {
		get => _costume;
		set => SetCostume(value);
	}
	private EntityCostume? _costume;

	public Model? Model { get; private set; }
	public bool IsLoaded => Model is not null;


	[ExportGroup("Dependencies")]
	[Export] public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			this.PropagateInject(_skeleton);
		}
	}
	private Skeleton3D? _skeleton;

	[Export] public AnimationPlayer? AnimationPlayer {
		get => _animationPlayer;
		private set {
			_animationPlayer = value;

			if (_animationPlayer is not null) {
				_animationPlayer.RootNode = _animationPlayer.GetPathTo(this);
			}
		}
	}
	private AnimationPlayer? _animationPlayer;

	[Export] public Gauge? Health {
		get => _health;
		set {
			if (_health == value) return;

			Callable onKill = Callable.From<float>(OnKill);
			NodeExtensions.SwapSignalEmitter(ref _health, value, Gauge.SignalName.Emptied, onKill);
		}
	}
	private Gauge? _health;


	[ExportGroup("State")]
	[Export] public EntityAction? CurrentAction { get; private set; }
	[Export] public EntityBehaviour? CurrentBehaviour { get; private set; }
	[Export] public Godot.Collections.Array<AttributeModifier> AttributeModifiers { get; private set; } = [];


	[ExportGroup("Movement")]
	[Export] public Vector3 Inertia = Vector3.Zero;
	[Export] public Vector3 Movement = Vector3.Zero;

	/// <summary>
	/// The forward direction in global space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="Forward"/> to match.
	/// </remarks>
	[Export] public Vector3 GlobalForward {
		get => _globalForward;
		set => _globalForward = value.Normalized();
	}
	private Vector3 _globalForward = Vector3.Forward;

	/// <summary>
	/// The forward direction in relative space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="GlobalForward"/> to match.
	/// </remarks>
	[Export] public Vector3 Forward {
		get => Transform.Basis.Inverse() * _globalForward;
		set => _globalForward = Transform.Basis * value;
	}

	[Signal] public delegate void DeathEventHandler(float fromHealth);



	protected Entity() : base() {
		CollisionLayer = Collisions.EntityCollisionLayer;

		Forward = Vector3.Forward;
	}
	public Entity(EntityCostume? costume) : this() {
		SetCostume(costume);
	}



	public void SetCostume(EntityCostume? newCostume) {
		EntityCostume? oldCostume = _costume;
		if (newCostume == oldCostume) return;

		_costume = newCostume;

		Callable.From<bool>(Load).CallDeferred(true);
	}

	public void ExecuteAction(EntityActionInfo action, bool forceExecute = false) {
		if (! forceExecute && ! ActionExtensions.CanCancel(CurrentAction)) return;

		CurrentAction?.QueueFree();
		CurrentAction = null;

		action.AfterExecute += () => CurrentAction = null;
		CurrentAction = action.Execute(this);

		CurrentAction.ParentTo(this);
	}


	public void SetBehaviour<TBehaviour>(TBehaviour? behaviour) where TBehaviour : EntityBehaviour {
		Callable.From(() =>{
			CurrentBehaviour?.Stop();

			behaviour?.Start(CurrentBehaviour?.IsQueuedForDeletion() ?? true ? null : CurrentBehaviour);
			CurrentBehaviour = behaviour?.SafeReparent(this);
		}).CallDeferred();
	}


	private void Move(double delta) {

		if (MotionMode == MotionModeEnum.Grounded) {
			if (
				IsOnFloor() && GetPlatformVelocity().IsZeroApprox()
				// && (lastStandableSurfaces.Count == 0 || GlobalPosition.DistanceSquaredTo(lastStandableSurfaces[^1]) > 0.25f)
			) {
				if (standableSurfaceBuffer.Count >= STANDABLE_SURFACE_BUFFER_SIZE) standableSurfaceBuffer.RemoveRange(0, standableSurfaceBuffer.Count - STANDABLE_SURFACE_BUFFER_SIZE);
				standableSurfaceBuffer.Add(GlobalPosition);
			}

			Inertia.Split(UpDirection, out Vector3 verticalInertia, out Vector3 horizontalInertia);

			if (IsOnFloor()) {
				verticalInertia = verticalInertia.SlideOnFace(UpDirection);
			}
			if (IsOnCeiling()) {
				verticalInertia = verticalInertia.SlideOnFace(-UpDirection);
			}

			Inertia = horizontalInertia + verticalInertia;

			if (MoveStep(delta)) {
				Movement = Vector3.Zero;
			}
		}

		Velocity = Inertia + Movement;

		MoveAndSlide();


		bool MoveStep(double delta) {
			if (Mathf.IsZeroApprox(Movement.LengthSquared()) || ! IsOnFloor()) return false;

			Vector3 movement = Movement * (float)delta;
			Vector3 destination = GlobalTransform.Origin + movement;

			KinematicCollision3D? stepObstacleCollision = MoveAndCollide(movement, true);

			float margin = Mathf.Epsilon;

			// // Down Step
			// if (stepObstacleCollision is null) {
			// 	margin += 4.5f;
			// }

			Vector3 sweepStart = destination;
			Vector3 sweepMotion = (Stats.StepHeight + margin) * -UpDirection;

			// Up Step
			if (stepObstacleCollision is not null) {
				sweepStart -= sweepMotion;
			}

			PhysicsTestMotionResult3D stepTestResult = new();
			bool findStep = PhysicsServer3D.BodyTestMotion(
				GetRid(),
				new() {
					From = GlobalTransform with { Origin = sweepStart },
					Motion = sweepMotion,
				},
				stepTestResult
			);

			if (! findStep) return false;
			if (stepObstacleCollision is not null && stepTestResult.GetColliderRid() != stepObstacleCollision.GetColliderRid()) return false;


			Vector3 point = stepTestResult.GetCollisionPoint();

			Vector3 destinationHeight = destination.Project(UpDirection);
			Vector3 pointHeight = point.Project(UpDirection);

			float stepHeightSquared = destinationHeight.DistanceSquaredTo(pointHeight);
			if (stepHeightSquared >= sweepMotion.LengthSquared()) return false;


			GlobalTransform = GlobalTransform with { Origin = destination - destinationHeight + pointHeight };
			// if (stepHeightSquared >= Mathf.Pow(stepHeight, 2f) / 4f) {
			// 	GD.Print("Step Found");
			// }

			return true;
		}
	}



	private void OnKill(float fromHealth) {
		EmitSignal(SignalName.Death, fromHealth);
	}

	public void VoidOut() {
		if (standableSurfaceBuffer.Count == 0) {
			GD.PushError("Could not Void out Properly");
			return;
		}

		standableSurfaceBuffer.RemoveRange(1, standableSurfaceBuffer.Count - 1);
		GlobalPosition = standableSurfaceBuffer[0];

		GD.Print($"Entity {Name} Voided out.");

		if (Health is not null) {
			Health.Amount -= Health.MaxAmount / 8f;
		}
	}

	public virtual void HandlePlayer(Player player) {
		if (healthBar is null && Health is not null) {
			healthBar = player.HudManager.AddInfo(HudPack.HealthBar);

			if (healthBar is not null) {
				healthBar.Value = Health;
			}
		}
		player.CameraController.SetEntityAsSubject(this);
		player.CameraController.MoveCamera(
			player.InputDevice.GetVector("look_left", "look_right", "look_down", "look_up") * player.InputDevice.Sensitivity
		);
	}

	public virtual void DisavowPlayer() {
		healthBar?.QueueFree();
		healthBar = null;
	}


	public void Load(bool forceReload = false) {
		if (IsLoaded && ! forceReload) return;

		Unload();

		Model = Costume?.Instantiate()?.ParentTo(this);

		if (Model is null) return;

		Model?.PropagateInject(Skeleton);
		Model?.PropagateInject(Handedness);
	}
	public void Unload() {
		Model?.QueueFree();
		Model = null;
	}


	private void UpdateHealth(bool keepRatio) {
		_health?.SetMaximum(AttributeModifiers.Get(Attributes.GenericMaxHealth).ApplyTo(Stats.MaxHealth), keepRatio);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		UpdateHealth(false);

		if (Engine.IsEditorHint()) return;

		Move(delta);
	}

	public override void _Ready() {
		base._Ready();

		UpdateHealth(true);

		if (_globalForward == Vector3.Zero) {
			_globalForward = GlobalBasis.Forward();
		}

		if (Engine.IsEditorHint()) return;

		if (CurrentBehaviour is null) {
			SetBehaviour(new BipedBehaviour(this));
		}
	}

	public override void _ExitTree() {
		base._ExitTree();
		healthBar?.QueueFree();
	}



	public ISaveData<Entity> Save() => new EntitySaveData<Entity>(this);



	[Serializable]
	public class EntitySaveData<T>(T data) : CostumableSaveData<Entity, T, EntityCostume>(data) where T : Entity {
		public ISaveData[] MiscData = [.. data.GetChildren().OfType<ISaveable>().Select(d => d.Save())];

		public override T? Load() {
			if (base.Load() is not T entity) return null;

			MiscData.ForEach(d => d.Load()?.ParentTo(entity));

			return entity;
		}
	}
}