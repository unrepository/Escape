using System.Diagnostics;
using Escape.Renderer;
using Silk.NET.Input;

namespace Escape.Input {
	
	public class InputMap : IDisposable, IUpdater {
		
		public string Id { get; }

		public Window Window { get; set; }
		public Dictionary<InputCombo[], InputAction> Actions { get; set; }

		private readonly List<Key> _currentKeyCombo = [];
		
		public InputMap(string id, Window window, Dictionary<InputCombo[], InputAction> actions) {
			Id = id;
			Window = window;
			Actions = actions;
			
			Debug.Assert(window.Input is not null);

			var keyboards = window.Input!.Keyboards;
			var mice = window.Input!.Mice;
			var gamepads = window.Input!.Gamepads;

			foreach(var keyboard in keyboards) {
				keyboard.KeyUp += _KeyUpHandler;
				keyboard.KeyDown += _KeyDownHandler;
			}
			
			UpdateManager.Add(this);
		}
		
		public void Update(TimeSpan delta) {
			foreach(var (combos, action) in Actions) {
				action.WasPressed = false;
				action.WasReleased = false;
				
				bool anyComboDown = false;

				foreach(var combo in combos) {
					if(
						_currentKeyCombo.SequenceEqual(combo.Keys)
						|| (!combo.Strict && combo.Keys.All(k => _currentKeyCombo.Contains(k)))
					) {
						anyComboDown = true;
						break;
					}
				}

				if(anyComboDown) {
					if(!action.IsDown) {
						action.IsDown = true;
						action.IsUp = false;
						action.WasPressed = true;
						action.OnPressed();
					}

					action.OnDown();
				} else {
					if(action.IsDown) {
						action.IsDown = false;
						action.IsUp = true;
						action.WasReleased = true;
						action.OnReleased();
					}

					action.OnUp();
				}
			}
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			UpdateManager.Remove(Id);
			
			var keyboards = Window.Input!.Keyboards;
			var mice = Window.Input!.Mice;
			var gamepads = Window.Input!.Gamepads;

			foreach(var keyboard in keyboards) {
				keyboard.KeyUp -= _KeyUpHandler;
				keyboard.KeyDown -= _KeyDownHandler;
			}
		}

		private void _KeyUpHandler(IKeyboard keyboard, Key key, int mod) {
			_currentKeyCombo.Remove(key);
		}
		
		private void _KeyDownHandler(IKeyboard keyboard, Key key, int mod) {
			_currentKeyCombo.Add(key);
		}
	}
}
