using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

public abstract class EntityAction : IDisposable, IInputReader {
	private bool _disposed = false;
	public event Action? OnDispose;


	public abstract bool IsCancellable { get; }
	public abstract bool IsKnockable { get; }



	public virtual void HandleInput(Player.InputInfo inputInfo) {}
	protected virtual void DisposeBehaviour() {}



	public void Dispose() {
		Callable.From( () => {
			Dispose(true);
			GC.SuppressFinalize(this);
		} ).CallDeferred();
	}

	private void Dispose(bool disposing) {
		if (_disposed) {
			return;
		}

		if (disposing) {
			OnDispose?.Invoke();
			DisposeBehaviour();
		}

		_disposed = true;
	}



	public abstract class Info {

		public abstract float PotentialDamage { get; }
		public abstract AttackType Type { get; }

		public event Action? BeforeExecute;
		public event Action? AfterExecute;


		public EntityAction Execute() {
			BeforeExecute?.Invoke();
			EntityAction attack = Build();
			attack.OnDispose += AfterExecute;

			BeforeExecute = null;
			AfterExecute = null;
			return attack;
		}
		protected abstract EntityAction Build();



		public static Info SelectAttack(Info[] attacks, IComparer<Info> priority, float skillLevel = 0.5f) {
			skillLevel = Mathf.Clamp(skillLevel, 0, 1);

			Array.Sort(attacks, priority);

			float skillMargin = skillLevel * attacks.Length;
			int weightedIndex = Mathf.RoundToInt(GD.Randf() * skillMargin);

			return attacks[weightedIndex];
		}

		[Flags]
		public enum AttackType : byte {
			Melee = 1 << 0,
			Projectile = 1 << 1,
			Parry = 1 << 2,
		}

		public static class Comparers {
			public readonly static IComparer<Info> PureDamage = new ComparisonComparer<Info>(
				(a, b) => a?.PotentialDamage.CompareTo(b?.PotentialDamage ?? 0) ?? 0
			);



			private class ComparisonComparer<T>(Comparison<T?> Comparison) : IComparer<T> where T : notnull {
				public int Compare(T? x, T? y) {
					return Comparison(x, y);
				}
			}
		}

	}
}