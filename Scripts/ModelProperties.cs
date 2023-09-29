using Godot;
using System;


using Attachment = EndlessSkies.Core.IModelAttachment.AttachmentPart;

namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class ModelProperties : Node {
    [Export] public Skeleton3D Skeleton { get; private set; }
    [Export] public Node3D Head { get; private set; }
    [Export] public Node3D HandR { get; private set; }
    [Export] public Node3D HandL { get; private set; }
    [Export] public Node3D FootR { get; private set; }
    [Export] public Node3D FootL { get; private set; }
    [Export] public Node3D WeaponR { get; private set; }
    [Export] public Node3D WeaponL { get; private set; }


    public Node3D GetAttachment(Attachment key) {
        return key switch {
            Attachment.Head => Head,
            Attachment.HandR => HandR,
            Attachment.HandL => HandL,
            Attachment.FootR => FootR,
            Attachment.FootL => FootL,
            Attachment.WeaponR => WeaponR,
            Attachment.WeaponL => WeaponL,
            _ => null,
        };
    }
}
