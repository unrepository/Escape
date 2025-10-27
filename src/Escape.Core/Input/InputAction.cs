namespace Escape.Core.Input {
	
	public class InputAction {
		
		public string Name { get; }

		public delegate void UpEventHandler(InputAction action);
		public delegate void DownEventHandler(InputAction action);
		public delegate void PressedEventHandler(InputAction action);
		public delegate void ReleasedEventHandler(InputAction action);

		public event UpEventHandler? Up;
		public event DownEventHandler? Down;
		public event PressedEventHandler? Pressed;
		public event ReleasedEventHandler? Released;

		public bool IsUp { get; internal set; } = false;
		public bool IsDown { get; internal set; } = false;
		public bool WasPressed { get; internal set; } = false;
		public bool WasReleased { get; internal set; } = false;
		
		public InputAction(string name) {
			Name = name;
		}

		public void OnUp() => Up?.Invoke(this);
		public void OnDown() => Down?.Invoke(this);
		public void OnPressed() => Pressed?.Invoke(this);
		public void OnReleased() => Released?.Invoke(this);
	}
}
