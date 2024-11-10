namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Persistence;


/// <summary>
/// The Entity is the Main part of the Framework and corresponds to any thing a Player is able to Control partially or entirely.
/// All Characters and Enemies are Entities.
/// </summary>
[Tool]
[GlobalClass]
[Injector]
public partial class Entity : CharacterBody3D, IPlayerHandler, IDamageable, IDamageDealer, ICostumable, IUIObject, IPersistent<Entity>, IItem<Entity>, ISerializationListener {
	public const int RECOVER_LOCATION_BUFFER_SIZE = 5;


	public uint? StyleSwitchBuffer;

	public readonly List<Vector3> RecoverLocationBuffer = new(RECOVER_LOCATION_BUFFER_SIZE);


	IDataKeyProvider<Entity> IItem<Entity>.KeyProvider => KeyProvider;
	[Export] private EntityResourceDataKey KeyProvider = new();

	[Export] public ItemUIData? UI { get; private set; }
	public string DisplayName => UI?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UI?.DisplayPortrait;


	public IDamageable? Damageable => this;


	[Export] public EntityStats Stats { get; private set; } = new();
	[Export] public HudPack HudPack { get; private set; } = new();

	[Export]
	[Injector]
	public virtual Handedness Handedness {
		get => _handedness;
		protected set {
			_handedness = value;
			this.PropagateInjection<Handedness>();
		}
	}
	private Handedness _handedness = Handedness.Right;


	[ExportGroup("Costume")]
	public CostumeHolder CostumeHolder => _costumeHolder ??= new CostumeHolder().SafeReparentAndSetOwner(this).SafeRename(nameof(CostumeHolder));
	[Export] private CostumeHolder? _costumeHolder;


	[ExportGroup("Dependencies")]
	[Export]
	[Injector]
	public virtual Skeleton3D? Skeleton {
		get => _skeleton;
		protected set {
			_skeleton = value;
			this.PropagateInjection<Skeleton3D>();
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


	[ExportGroup("State")]
	[Export] public Action? CurrentAction { get; private set; }
	[Export] public EntityBehaviour? CurrentBehaviour { get; private set; }
	protected virtual Func<EntityBehaviour> DefaultBehaviour => () => new BipedBehaviour(this);


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

	private GaugeControl? healthBar;


	public IWeapon? Weapon {
		get => _weapon;
		set {
			_weapon = value;
			if (_weapon is Node nodeWeapon) {
				nodeWeapon.SafeRename("Weapon");
			}
		}
	}
	private IWeapon? _weapon;


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



	[Signal] public delegate void DeathEventHandler(float fromHealth);


	public Entity() : base() {
		CollisionLayer = CollisionLayers.Entity;
		CollisionMask = CollisionLayers.Terrain;

		uint movingPlatformLayers = uint.MaxValue & ~(CollisionLayers.Entity | CollisionLayers.Water | CollisionLayers.Interactable | CollisionLayers.Damage);
		PlatformFloorLayers = movingPlatformLayers;
		PlatformWallLayers = movingPlatformLayers;

		Forward = Vector3.Forward;
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


	public virtual IEnumerable<IUIObject> GetSubObjects() => GetChildren().OfType<IUIObject>();
	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];

	private void GetWeapon() => Weapon = GetChildren().OfType<IWeapon>().FirstOrDefault();

	public void Damage(ref DamageData data) {
		if (Health is null) return;

		// TODO: damage reduction, absorption, etc...

		Health.Value -= data.Amount;
		GD.Print($"Entity {Name} took {data.Amount} Damage");
	}

	public void Kill() {
		if (Health is null) return;
		Health.Value = 0;
	}


	private void OnKill(float fromHealth) {
		EmitSignalDeath(fromHealth);
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
		HandleHealthBar(player);
		HandleWeaponInput(player);


		void HandleHealthBar(Player player) {
			if (healthBar is null && Health is not null) {
				healthBar = player.HudManager.AddInfo(HudPack.HealthBar);

				if (healthBar is not null) {
					healthBar.Value = Health;
				}
			}
		}

		void HandleWeaponInput(Player player) {
			if (_weapon is null) return;

			uint maxWeaponStyle = (uint)Math.Min((int)(Weapon?.MaxStyle ?? 0) + 1, Inputs.SwitchWeaponActions.Length);
			uint? bufferStyle = null;
			for (uint i = 0; i < maxWeaponStyle; i++) {
				if (player.InputDevice.IsActionJustPressed(Inputs.SwitchWeaponActions[i])) {
					GD.PrintS(i, Inputs.SwitchWeaponActions[i]);
					bufferStyle = i;
					break;
				}
			}

			if (bufferStyle.HasValue) {
				if (CurrentAction is Attack attack && !attack.CanCancel()) {
					StyleSwitchBuffer = bufferStyle.Value;
				}
				else {
					_weapon.Style = bufferStyle.Value;
				}
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

		this.PropagateInjection<Entity>();
		this.PropagateInjection<Skeleton3D>();
		this.PropagateInjection<Handedness>();

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

	public override void _Process(double delta) {
		base._Process(delta);
		HandleWeapon();


		void HandleWeapon() {
			if (_weapon is null) return;
			if (CurrentAction is Attack attack && !attack.CanCancel()) return;

			if (StyleSwitchBuffer is uint bufferedStyle) {
				_weapon.Style = bufferedStyle;
			}
		}
	}

	public override void _Notification(int what) {
		base._Notification(what);
		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				GetWeapon();
				break;
		}
	}

	public virtual void OnBeforeSerialize() { }
	public virtual void OnAfterDeserialize() {
		GetWeapon();
	}


	public virtual void AwardDamage(in DamageData data, IDamageable? target) {
		GD.Print($"{Name} hit {(target as Node)?.Name} for {data.Amount} damage.");
	}



	public virtual IPersistenceData<Entity> Save() => new EntitySaveData<Entity>(this);
	[Serializable]
	public class EntitySaveData<T>(T entity) : ItemPersistenceData<Entity>(entity) where T : Entity {
		public IPersistenceData[] MiscData = [.. entity.GetChildren().OfType<IPersistent>().Select(d => d.Save())];

		protected override void LoadInternal(Entity item) {
			MiscData.ForEach(d => (d.Load() as Node)?.ParentTo(item));
		}
	}
}