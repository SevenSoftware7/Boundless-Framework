using Godot;
using SevenGame.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Character : Loadable {

    [Export] public Node3D? Collisions { get; private set; }
    [Export] public Skeleton3D? Armature { get; private set; }

    [Export] public CharacterData Data {
        get => _data;
        private set => _data ??= value;
    }
    private CharacterData _data;
    

    [ExportGroup("Costume")]
    [Export] private CharacterModel? CharacterModel;

    [Export] public CharacterCostume? CharacterCostume {
        get => CharacterModel?.Costume;
        private set => SetCostume(value);
    }


    public Basis CharacterRotation { get; private set; } = Basis.Identity;


    [Signal] public delegate void CostumeChangedEventHandler(CharacterCostume? newCostume, CharacterCostume? oldCostume);



    public Character() : base() {
        Data ??= null !;

        Name = nameof(Character);
    }
    public Character(CharacterData data, CharacterCostume? costume) : base() {
        ArgumentNullException.ThrowIfNull(data);

        Data = data;
        SetCostume(costume);

        Name = nameof(Character);
    }



    public void SetCostume(CharacterCostume? costume, bool forceLoad = false) {
        CharacterCostume? oldCostume = CharacterCostume;
        if ( costume == oldCostume ) return;

#if TOOLS
        Callable.From(SetCostume).CallDeferred();
        void SetCostume() =>
#endif
        this.UpdateLoadable<CharacterModel, CharacterCostume>()
            .WithConstructor(() => costume?.Instantiate().SetOwnerAndParentTo(this))
            .BeforeLoad((model) => model.SetSkeleton(Armature))
            .WhenFinished((_) => EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!))
            .Execute(ref CharacterModel);
    }


    protected override bool LoadModelImmediate() {
        Node parent = GetParent();
        if ( parent is null ) return false;
        if ( Data is null ) return false;

        Collisions = Data.CollisionScene?.Instantiate() as Node3D;
        if ( Collisions is not null ) {
            Collisions.Name = nameof(Collisions);
            Collisions.SetOwnerAndParentTo(parent);
        }

        Armature = Data.SkeletonScene?.Instantiate() as Skeleton3D;
        if ( Armature is not null ) {
            Armature.Name = nameof(Armature);
            Armature.SetOwnerAndParentTo(this);
        }

        CharacterModel?.LoadModel();

        RefreshRotation();

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Collisions?.UnparentAndQueueFree();
        Collisions = null;

        Armature?.UnparentAndQueueFree();
        Armature = null;

        CharacterModel?.UnloadModel();

        return true;
    }


    public void RotateTowards(Basis target, double delta, float speed = 12f) {
        CharacterRotation = CharacterRotation.SafeSlerp(target, (float)delta * speed);

        RefreshRotation();
    }

    protected virtual void RefreshRotation() {
        Transform = new(CharacterRotation, Transform.Origin);

        if ( Collisions is null ) return;

        Collisions.Transform = new(CharacterRotation, Collisions.Transform.Origin);
    }

}
