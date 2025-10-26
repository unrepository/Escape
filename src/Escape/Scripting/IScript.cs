namespace Escape.Scripting {
	
	public interface IScript {
		
		public string Name { get; }
		public string Source { get; }

		public object? Call(string function, object?[] arguments);
	}
}
