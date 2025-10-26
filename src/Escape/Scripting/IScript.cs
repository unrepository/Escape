namespace Escape.Scripting {
	
	public interface IScript : IDisposable {
		
		public string Name { get; }
		public string Source { get; }
		
		public Language Type { get; }

		public object? Call(string function, object?[] arguments);
		public object? Call(FunctionCall function, object?[] arguments);

		public enum Language {
			
			JavaScript,
			CSharp
		}

		public enum FunctionCall {
			
			OnInitialize,
			OnDeinitialize,
			OnUpdate,
			OnRender
		}
	}
}
