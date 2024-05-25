namespace LandlessSkies.Vanilla;

using System.IO;
using System.Threading.Tasks;
using Godot;
using KGySoft.Serialization.Binary;
using LandlessSkies.Core;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class TestInteractable : Interactable {
	public override string InteractLabel => "Interact";
	public override float? MinLookIncidence => 0f;

	private Task<ISaveData<Entity>?>? SaveLoadTask;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		GD.Print($"Entity {entity.Name} interacted with {Name}, shape {GetShape3D(shapeIndex)?.Name} (index {shapeIndex})");

		if (player is null) return;

		ISaveData<Entity>? savedEntity = entity.Save();

		SaveLoadTask = Task.Run(() => {
			string path = @$"{OS.GetUserDataDir()}/SaveData1.dat";
			BinarySerializationFormatter formatter = new(BinarySerializationOptions.CompactSerializationOfStructures);

			using (FileStream stream = new(path, FileMode.Create)) {
				formatter.SerializeToStream(stream, savedEntity);
			}
			using (FileStream stream = new(path, FileMode.Open)) {
				savedEntity = formatter.DeserializeFromStream<ISaveData<Entity>?>(stream);
			}

			return savedEntity;
		});
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (SaveLoadTask?.IsCompletedSuccessfully ?? false) {
			Entity? clonedEntity = SaveLoadTask.Result?.Load()?.SetOwnerAndParent(this);
			if (clonedEntity is not null) {
				clonedEntity.GlobalTransform = GlobalTransform;

				// player.Entity = clonedEntity;
			}

			SaveLoadTask = null;
		}
	}

	public override bool IsInteractable(Entity entity) {
		return true;
	}
}