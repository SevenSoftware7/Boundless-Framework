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
		Callable.From(DisposeAction).CallDeferred();

		void DisposeAction() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
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
	}
}