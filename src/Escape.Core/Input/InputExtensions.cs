using System.Diagnostics;
using Escape.Renderer;
using Silk.NET.Input;

namespace Escape.Core.Input {
	
	public static class InputExtensions {

		public static IReadOnlyList<IKeyboard> GetKeyboards(this Window window) {
			Debug.Assert(window.Input is not null);
			return window.Input.Keyboards;
		}
		
		public static IKeyboard GetKeyboard(this Window window, int index = 0) {
			return window.GetKeyboards()[index];
		}

		public static IReadOnlyList<IMouse> GetMice(this Window window) {
			Debug.Assert(window.Input is not null);
			return window.Input.Mice;
		}

		public static IMouse GetMouse(this Window window, int index = 0) {
			return window.GetMice()[index];
		}

		public static IReadOnlyList<IGamepad> GetGamepads(this Window window) {
			Debug.Assert(window.Input is not null);
			return window.Input.Gamepads;
		}

		public static IGamepad GetGamepad(this Window window, int index = 0) {
			return window.GetGamepads()[index];
		}
	}
}
