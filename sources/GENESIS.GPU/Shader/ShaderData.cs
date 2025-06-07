using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GENESIS.GPU.Shader {
	
	public unsafe class ShaderData<T> : IDisposable {
		
		public Shader? Owner { get; internal set; }
		
		public uint Id { get; internal set; }
		public uint Binding { get; set; }

		/// <summary>
		/// Whether to automatically push data changes to the owner shader
		/// </summary>
		public bool AutoUpdate { get; set; } = true;

		public T Data {
			get => InnerData;
			set {
				InnerData = value;
				if(AutoUpdate) Owner?.UpdateData(this);
			}
		}

		//public T Data;
		public uint Size { get; set; }

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
