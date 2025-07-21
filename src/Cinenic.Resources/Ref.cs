namespace Cinenic.Resources {
	
	public class Ref<T> : IDisposable where T : class, IRefCounted {

		public T ReferencedObject { private get; init; }
		public bool IsValid => ReferencedObject.IsValidObject;
		
		public delegate void FreedEventHandler(Ref<T> sender);
		public event FreedEventHandler? Freed;

		public delegate void ReloadedEventHandler(Ref<T> sender);
		public event ReloadedEventHandler? Reloaded;
		
		public T Get() => ReferencedObject;

		public Ref(T referencedObject) {
			ReferencedObject = referencedObject;
			referencedObject.NewReference();
		}
		
		~Ref() {
			Dispose();
		}
		
		public void Dispose() {
			ReferencedObject.FreeReference();

			if(!ReferencedObject.IsValidObject) {
				Freed?.Invoke(this);
			}
			
			GC.SuppressFinalize(this);
		}

		public static implicit operator T(Ref<T> reference) => reference.Get();
	}
}
