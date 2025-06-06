using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GENESIS.GPU.Shader {
	
	public unsafe class ShaderData<T> : IDisposable {
		
		public IShader? Owner { get; internal set; }
		
		public uint Id { get; internal set; }
		public uint Binding { get; set; }

		public T Data {
			get {
				if(MappedMemory is null) {
					return InnerData;
				}
		
				return *MappedMemory;
			}
			set {
				if(MappedMemory is null) {
					InnerData = value;
					return;
				}
				
				Unsafe.CopyBlockUnaligned(MappedMemory, &value, Size);
			}
		}

		//public T Data;
		public uint Size { get; init; }

		internal T* MappedMemory { get; set; }
		internal T InnerData;

		public void Dispose() {
			MappedMemory = null;

			Owner?.DeallocatedDataObjects.Add(Id);
			Owner = null;
			
			Id = 0;
			Binding = 0;
			InnerData = default;
		}
	}
}
