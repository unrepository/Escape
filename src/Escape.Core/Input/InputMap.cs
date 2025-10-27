using System.Diagnostics;
using Escape.Renderer;
using Silk.NET.Input;

namespace Escape.Core.Input {
	
	// TODO gamepads
	public class InputMap : IDisposable, IUpdater {
		
		public string Id { get; }

		public Window Window { get; set; }
		public Dictionary<InputCombo[], InputAction> Actions { get; set; }

		private readonly List<Key> _currentKeyCombo = [];
		private readonly List<MouseButton> _currentMouseButtonCombo = [];
		private MouseScrollWheel? _currentScrollWheel = null;
		
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

			foreach(var mouse in mice) {
				mouse.MouseUp += _MouseUpHandler;
				mouse.MouseDown += _MouseDownHandler;
				mouse.Scroll += _MouseScrollHandler;
			}
			
			UpdateManager.Add(this);
		}

		public InputAction? GetAction(string name) {
			return Actions.Values.SingleOrDefault(action => action.Name == name);
		}
		
		public void Update(TimeSpan delta) {
			foreach(var (combos, action) in Actions) {
				action.WasPressed = false;
				action.WasReleased = false;
				
				bool anyComboDown = false;

				foreach(var combo in combos) {
					bool KeyCondition() => 
						_currentKeyCombo.SequenceEqual(combo.Keys)
						|| (!combo.Strict && combo.Keys.All(k => _currentKeyCombo.Contains(k)));

					bool MouseButtonCondition() =>
						_currentMouseButtonCombo.SequenceEqual(combo.MouseButtons)
						|| (!combo.Strict && combo.MouseButtons.All(m => _currentMouseButtonCombo.Contains(m)));

					bool MouseScrollWhellCondition() =>
						_currentScrollWheel == combo.MouseScrollWheel;

					if(
						(combo.Keys?.Length > 0 && KeyCondition())
						|| (combo.MouseButtons?.Length > 0 && MouseButtonCondition())
						|| (combo.MouseScrollWheel is not null && MouseScrollWhellCondition())
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

			_currentScrollWheel = null;
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
			
			foreach(var mouse in mice) {
				mouse.MouseUp -= _MouseUpHandler;
				mouse.MouseDown -= _MouseDownHandler;
				mouse.Scroll -= _MouseScrollHandler;
			}
		}

		private void _KeyUpHandler(IKeyboard keyboard, Key key, int mod) {
			_currentKeyCombo.Remove(key);
		}
		
		private void _KeyDownHandler(IKeyboard keyboard, Key key, int mod) {
			_currentKeyCombo.Add(key);
		}
		
		private void _MouseUpHandler(IMouse mouse, MouseButton button) {
			_currentMouseButtonCombo.Remove(button);
		}
		
		private void _MouseDownHandler(IMouse mouse, MouseButton button) {
			_currentMouseButtonCombo.Add(button);
		}
		
		private void _MouseScrollHandler(IMouse mouse, ScrollWheel wheel) {
			_currentScrollWheel = wheel.Y > 0 ? MouseScrollWheel.Up : MouseScrollWheel.Down;
		}
	}
}
