namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Utility;

/// <summary>
/// The Entity is the Main part of the Framework and corresponds to any thing a Player is able to Control partially or entirely.
/// All Characters and Enemies are Entities.
/// </summary>
[Tool]
[GlobalClass]
public partial class Entity : CharacterBody3D, IPlayerHandler, IDamageable, IDamageDealer, ICostumable, ICustomizable, ISaveable<Entity>, IInjectionProvider<Entity?>, IInjectionProvider<Skeleton3D?>, IInjectionProvider<Handedness>, ISerializationListener {
	public readonly List<Vector3> RecoverLocationBuffer = [];
	public const int RECOVER_LOCATION_BUFFER_SIZE = 5;

	private GaugeControl? healthBar;

	[Export] public string DisplayName { get; private set; } = string.Empty;
	public Texture2D? DisplayPortrait => CostumeHolder?.Costume?.DisplayPortrait;


	public IDamageable? Damageable => this;

	[Export] public EntityStats Stats { get; private set; } = new();
	[Export] public HudPack HudPack { get; private set; } = new();

	[Export]
	public virtual Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;
			if (IsNodeReady()) {
				this.PropagateInject<Handedness>();
			}
		}
	}
	private Handedness _handedness = Handedness.Right;


	[ExportGroup("Costume")]
	[Export] public CostumeHolder? CostumeHolder { get; set; }


	[ExportGroup("Dependencies")]
	[Export]
	public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			if (IsNodeReady()) {
				this.PropagateInject<Skeleton3D?>();
			}
		}
	}
	private Skeleton3D? _skeleton;

	[Export]
	public AnimationPlayer? AnimationPlayer {
		get => _animationPlayer;
		private set {
			_animationPlayer = value;

			if (_animationPlayer is not null) {
				_animationPlayer.RootNode = _animationPlayer.GetPathTo(this);
			}
		}
	}
	private AnimationPlayer? _animationPlayer;

	[Export]
	public Gauge? Health {
		get => _health;
		set {
			if (_health == value) return;

			Callable onKill = Callable.From<float>(OnKill);
			NodeExtensions.SwapSignalEmitter(ref _health, value, Gauge.SignalName.Emptied, onKill);
		}
	}
	private Gauge? _health;


	[ExportGroup("State")]
	[Export] public Action? CurrentAction { get; private set; }
	[Export] public EntityBehaviour? CurrentBehaviour { get; private set; }
	[Export]
	private Godot.Collections.Array<AttributeModifier> _attributeModifiers {
		get => [.. AttributeModifiers, null];
		set => AttributeModifiers.Set([.. value.Where(a => a is not null)]);
	}
	public readonly AttributeModifierCollection AttributeModifiers = [];


	[ExportGroup("Movement")]
	[Export] public Vector3 Inertia = Vector3.Zero;
	[Export] public Vector3 Movement = Vector3.Zero;

	/// <summary>
	/// The forward direction in global space of the Entity.
	/// </summary>
	/// <remarks>
	/// Editing this value also changes <see cref="Forward"/> to match.
	/// </remarks>
	[Export]
	public Vector3 GlobalForward {
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
	[Export]
	public Vector3 Forward {
		get => Transform.Basis.Inverse() * _globalForward;
		set => _globalForward = Transform.Basis * value;
	}

	[ExportGroup("")]
	[Export]
	public Node3D? CenterOfMass { get; private set; }

	protected virtual Func<EntityBehaviour> DefaultBehaviour => () => new BipedBehaviour(this);


	[Signal] public delegate void DeathEventHandler(float fromHealth);


	protected Entity() : base() {
		CollisionLayer = CollisionLayers.Entity;
		CollisionMask = CollisionLayers.Terrain;

		uint movingPlatformLayers = uint.MaxValue & ~(CollisionLayers.Entity | CollisionLayers.Water | CollisionLayers.Interactable | CollisionLayers.Damage);
		PlatformFloorLayers = movingPlatformLayers;
		PlatformWallLayers = movingPlatformLayers;

		Forward = Vector3.Forward;
	}
	public Entity(EntityCostume? costume = null) : this() {
		CostumeHolder = new CostumeHolder(costume).ParentTo(this);
	}


	public bool ExecuteAction(Action.Builder builder, bool forceExecute = false) => ExecuteAction(new Action.Wrapper(builder), forceExecute);
	public bool ExecuteAction(Action.Wrapper action, bool forceExecute = false) {
		if (!forceExecute && !CurrentAction.CanCancel()) return false;

		CurrentAction?.Stop();
		CurrentAction = null;

		action.AfterExecute += () => CurrentAction = null;

		CurrentAction = action.Create(this).ParentTo(this).SafeRename("Action");

		CurrentAction.Start();

		return true;
	}

	public void SetBehaviour<TBehaviour>(Func<TBehaviour?> creator) where TBehaviour : EntityBehaviour =>
		SetBehaviour<TBehaviour, TBehaviour>(creator);

	public void SetBehaviour<TDefault, TFallback>(Func<TFallback?> creator) where TDefault : EntityBehaviour where TFallback : TDefault =>
		SetBehaviour(GetChildren().OfType<TDefault>().FirstOrDefault() ?? creator.Invoke());

	public void SetBehaviourOrReset<TBehaviour>() where TBehaviour : EntityBehaviour =>
		SetBehaviour(GetChildren().OfType<TBehaviour>().FirstOrDefault() ?? DefaultBehaviour.Invoke());

	public void SetBehaviour<TBehaviour>(TBehaviour? behaviour) where TBehaviour : EntityBehaviour {
		CurrentBehaviour?.Stop(behaviour);

		EntityBehaviour? oldBehaviour = CurrentBehaviour;
		CurrentBehaviour = null;

		if (behaviour is null) return;

		behaviour.SafeReparentAndRename(this, "Behaviour");

		behaviour.Start(oldBehaviour);

		CurrentBehaviour = behaviour;
	}


	public virtual List<ICustomization> GetCustomizations() => [];
	public virtual List<ICustomizable> GetSubCustomizables() => [.. GetChildren().OfType<ICustomizable>()];

	public void Damage(float amount) {
		if (Health is null) return;
		// TODO: damage reduction, absorption, etc...
		Health.Value -= amount;
		GD.Print($"Entity {Name} took {amount} Damage");
	}

	public void Kill() {
		if (Health is null) return;
		Health.Value = 0;
	}


	private void OnKill(float fromHealth) {
		EmitSignal(SignalName.Death, fromHealth);
	}

	public void VoidOut() {
		if (RecoverLocationBuffer.Count == 0) {
			GD.PushError("Could not Void out Properly, falling back to World Origin");
			GlobalPosition = Vector3.Zero;
			return;
		}
		else {
			RecoverLocationBuffer.RemoveRange(1, RecoverLocationBuffer.Count - 1);
			GlobalPosition = RecoverLocationBuffer[0];

			GD.Print($"Entity {Name} Voided out.");
		}

		if (Health is not null) {
			Health.Value -= Health.Maximum / 8f;
		}

		Inertia = Vector3.Zero;
	}

	public virtual void HandlePlayer(Player player) {
		if (healthBar is null && Health is not null) {
			healthBar = player.HudManager.AddInfo(HudPack.HealthBar);

			if (healthBar is not null) {
				healthBar.Value = Health;
			}
		}
	}

	public virtual void DisavowPlayer() {
		healthBar?.QueueFree();
		healthBar = null;
	}


	private void UpdateHealth(bool keepRatio) {
		_health?.SetMaximum(AttributeModifiers.ApplyTo(Attributes.GenericMaxHealth, Stats.MaxHealth), keepRatio);
	}
	private void OnHealthModifiersUpdate(EntityAttribute attribute) {
		if (attribute == Attributes.GenericMaxHealth) {
			UpdateHealth(false);
		}
	}

	public override void _Ready() {
		base._Ready();

		this.PropagateInject<Entity?>();
		this.PropagateInject<Skeleton3D?>();
		this.PropagateInject<Handedness>();

		UpdateHealth(true);

		AttributeModifiers.OnModifiersUpdated += OnHealthModifiersUpdate;

		if (_globalForward == Vector3.Zero) {
			_globalForward = GlobalBasis.Forward();
		}

		if (Engine.IsEditorHint()) return;

		if (CurrentBehaviour is null) {
			SetBehaviour(DefaultBehaviour);
		}
		else {
			CurrentBehaviour?.Start();
		}

	}

	public override void _ExitTree() {
		base._ExitTree();
		healthBar?.QueueFree();
	}

	public virtual void OnBeforeSerialize() { }
	public virtual void OnAfterDeserialize() {
		Callable.From(() => {
			this.PropagateInject<Entity?>();
			this.PropagateInject<Skeleton3D?>();
			this.PropagateInject<Handedness>();
		}).CallDeferred();
	}


	public ISaveData<Entity> Save() => new EntitySaveData<Entity>(this);

	Entity? IInjectionProvider<Entity?>.GetInjection() => this;
	Skeleton3D? IInjectionProvider<Skeleton3D?>.GetInjection() => Skeleton;
	Handedness IInjectionProvider<Handedness>.GetInjection() => Handedness;

	public virtual void AwardDamage(float amount, IDamageDealer.DamageType type, IDamageable target) {
		GD.Print($"{Name} hit {(target as Node)?.Name} for {amount} damage.");
	}



	[Serializable]
	public class EntitySaveData<T>(T entity) : CostumableSaveData<Entity, EntityCostume>(entity) where T : Entity {
		public ISaveData[] MiscData = [.. entity.GetChildren().OfType<ISaveable>().Select(d => d.Save())];

		public override T? Load() {
			if (base.Load() is not T entity) return null;

			MiscData.ForEach(d => (d.Load() as Node)?.ParentTo(entity));

			return entity;
		}
	}
}