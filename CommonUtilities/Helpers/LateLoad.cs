using System;

namespace IngameScript.CommonUtilities.Helpers {
	/// <summary>
	/// This is basically just the Lazy type
	/// </summary>
	public class LateLoad<T> {
		public T Value {
			get {
				CheckForAndLoadValue();
				return _value;
			}
		}

		private T _value;
		private Func<T> _setup;

		public LateLoad(Func<T> setup) {
			_setup = setup;
		}

		private void CheckForAndLoadValue() {
			if (_value != null) {
				return;
			}

			_value = _setup.Invoke();
		}
	}
}