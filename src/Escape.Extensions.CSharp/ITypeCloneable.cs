namespace Escape.Extensions.CSharp {
	
	public interface ITypeCloneable<T> : ICloneable {

		public new T Clone();
		object ICloneable.Clone() => Clone();
	}
}
