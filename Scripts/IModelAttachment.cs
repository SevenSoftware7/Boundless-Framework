using Godot;
using System;


namespace EndlessSkies.Core;

public interface IModelAttachment {
    Node3D RootAttachment { get; }
    Skeleton3D Skeleton { get; }
    // Animator animator { get; }

    Node3D GetAttachment(AttachmentPart key);



    public enum AttachmentPart {
        Head,
        HandR,
        HandL,
        FootR,
        FootL,
        WeaponR,
        WeaponL,
    }

}
