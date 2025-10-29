using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Escape.Renderer {
	
	// TODO why do we need all this hash and equation shit (we don't)
	public struct Renderable : IEquatable<Renderable> {
		
		public Guid Id { get; init; } = Guid.NewGuid();
		public Model? Model { get; set; }
		
		private static readonly SHA1 _hash = SHA1.Create();
		
		public Renderable(ulong id, Model? model) : this(id.ToString(), model) { }

		public Renderable(string id, Model? model) : this(model) {
			byte[] idBytes = Encoding.UTF8.GetBytes(id);
			byte[] hashBytes = _hash.ComputeHash(idBytes);
			byte[] guidBytes = new byte[16];
			
			Array.Copy(hashBytes, guidBytes, 16);
			
			// UUID version 5
			guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
			
			// RFC 4122
			guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

			Id = new Guid(guidBytes);
		}
		
		public Renderable(Guid id, Model? model) : this(model) {
			Id = id;
		}
		
		public Renderable(Model? model) {
			Model = model;
		}
		
		public Renderable() { }

		public override bool Equals([NotNullWhen(true)] object? o) => o is Renderable obj && obj.Id == Id;
		public bool Equals(Renderable other) => Id.Equals(other.Id);

		public static bool operator ==(Renderable a, Renderable b) => a.Equals(b);
		public static bool operator !=(Renderable a, Renderable b) => !(a == b);
		
		public override int GetHashCode() => Id.GetHashCode();
		
		public override string ToString() {
			return $"[Id={Id} Model={Model}]";
		}
    }
}
