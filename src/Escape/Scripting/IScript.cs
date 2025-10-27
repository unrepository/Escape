using System.Reflection;

namespace Escape.Scripting {
	
	public interface IScript : IDisposable {
		
		public string Name { get; }
		public string Source { get; }
		
		//public Language Type { get; }

		public void Construct(Type[] types, object?[] arguments);
		public object? Call(string function, object?[] arguments);
		public object? Call(FunctionCall call, object?[] arguments);

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
