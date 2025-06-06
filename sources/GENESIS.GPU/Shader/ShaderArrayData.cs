using System.Runtime.CompilerServices;

namespace GENESIS.GPU.Shader {
	
	public unsafe class ShaderArrayData<T> : ShaderData<T> {

		public new T[] Data {
			get {
				if(MappedMemory is null) {
					return InnerData;
				}
		
				return *(T[]*) MappedMemory;
			}
			set {
				if(MappedMemory is null) {
					InnerData = value;
					return;
				}
				
				Unsafe.CopyBlockUnaligned(MappedMemory, &value[0], Size);
			}
		}

		internal new T[] InnerData;
	}
}
