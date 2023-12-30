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



	public interface IInfo {
		Action? BeforeExecute { get; set; }
		Action? AfterExecute { get; set; }


		EntityAction Build();
	}
}

public static class EntityActionExtensions {
	public static EntityAction Execute(this EntityAction.IInfo info) {
		info.BeforeExecute?.Invoke();
		EntityAction action = info.Build();
		action.OnDispose += info.AfterExecute;

		return action;
	}
}