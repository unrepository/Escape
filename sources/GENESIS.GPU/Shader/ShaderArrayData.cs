using System.Runtime.CompilerServices;

namespace GENESIS.GPU.Shader {
	
	public class ShaderArrayData<T> : ShaderData<T> {

		public new T[] Data {
			get => InnerData;
			set {
				InnerData = value;
				if(AutoUpdate) Owner?.UpdateData(this);
			}
		}

		internal new T[] InnerData;
	}
}
