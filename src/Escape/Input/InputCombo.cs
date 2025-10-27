using Silk.NET.Input;

namespace Escape.Input {
	
	public struct InputCombo {

		public Key[]? Keys {
			get => field;
			set {
				if(value?.Length <= 1) Strict = false;
				else Strict = true;

				field = value;
			}
		} = null;
		
		public MouseButton[]? MouseButtons { get; set; } = null;
		public MouseScrollWheel? MouseScrollWheel { get; set; } = null;

		/// <summary>
		/// Whether strict matching should be used for this combo.
		/// When disabled, any key order works for matching as long as all keys are pressed.
		/// This is disabled by default for single-key inputs.
		/// </summary>
		public bool Strict { get; set; } = true;

		public InputCombo(params Key[] keys) {
			Keys = keys;
		}

		public InputCombo(params MouseButton[] mouseButtons) {
			MouseButtons = mouseButtons;
		}

		public InputCombo(MouseScrollWheel mouseScrollWheel) {
			MouseScrollWheel = mouseScrollWheel;
		}
	}
	
	public enum MouseScrollWheel {
			
		Up,
		Down
	}
}
