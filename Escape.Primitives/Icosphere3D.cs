using System.Numerics;
using Escape.Renderer;

namespace Escape.Primitives {
	
	public class Icosphere3D : Primitive {

		private const float PHI = 1.618033988749f;
		
		public Icosphere3D(Vector3 position, float radius, int subdivisions) : base(position) {
			var vertices = new Vector3[] {
				Vector3.Normalize(new(-1f,  PHI,   0f)),
				Vector3.Normalize(new( 1f,  PHI,   0f)),
				Vector3.Normalize(new(-1f, -PHI,   0f)),
				Vector3.Normalize(new( 1f, -PHI,   0f)),
				Vector3.Normalize(new( 0f, -1f ,  PHI)),
				Vector3.Normalize(new( 0f,  1f ,  PHI)),
				Vector3.Normalize(new( 0f, -1f , -PHI)),
				Vector3.Normalize(new( 0f,  1f , -PHI)),
				Vector3.Normalize(new( PHI,  0f, -1f )),
				Vector3.Normalize(new( PHI,  0f,  1f )),
				Vector3.Normalize(new(-PHI,  0f, -1f )),
				Vector3.Normalize(new(-PHI,  0f,  1f ))
			};

			var indices = new uint[] {
				0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
				1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
				3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
				4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
			};
			
			var (subdividedVertices, subdividedIndices) = Subdivide(vertices, indices, subdivisions);

			// project vertices onto sphere
			var sphereVertices = new Vector3[subdividedVertices.Length];
			
			for(int i = 0; i < subdividedVertices.Length; i++) {
				sphereVertices[i] = Vector3.Normalize(subdividedVertices[i]) * radius;
			}

			// apply vertices and indices to instance
			Vertices = new Vertex[subdividedVertices.Length];

			for(int i = 0; i < Vertices.Length; i++) {
				Vertices[i] = new Vertex {
					Position = subdividedVertices[i]
				};
			}

			Indices = subdividedIndices;
		}
		
		private static (Vector3[] vertices, uint[] indices) Subdivide(Vector3[] vertices, uint[] indices, int subdivisions) {
	        var edgeLookup = new Dictionary<(uint, uint), uint>();
	        var newVertices = new List<Vector3>(vertices);
	        var newIndices = new List<uint>();

	        for(int i = 0; i < subdivisions; i++) {
	            newIndices.Clear();
	            edgeLookup.Clear();

	            for(int j = 0; j < indices.Length; j += 3) {
	                uint i0 = indices[j];
	                uint i1 = indices[j + 1];
	                uint i2 = indices[j + 2];

	                uint a = GetMidpoint(i0, i1, ref newVertices, ref edgeLookup, vertices);
	                uint b = GetMidpoint(i1, i2, ref newVertices, ref edgeLookup, vertices);
	                uint c = GetMidpoint(i2, i0, ref newVertices, ref edgeLookup, vertices);

	                newIndices.Add(i0);
					newIndices.Add(a);
					newIndices.Add(c);
					
	                newIndices.Add(i1);
					newIndices.Add(b);
					newIndices.Add(a);
					
	                newIndices.Add(i2);
					newIndices.Add(c);
					newIndices.Add(b);
					
	                newIndices.Add(a);
					newIndices.Add(b);
					newIndices.Add(c);
	            }

	            vertices = newVertices.ToArray();
	            indices = newIndices.ToArray();
	        }

	        return (vertices, indices);
	    }

	    private static uint GetMidpoint(uint i0, uint i1, ref List<Vector3> vertices, ref Dictionary<(uint, uint), uint> edgeLookup, Vector3[] oVertices) {
	        uint smaller = Math.Min(i0, i1);
	        uint larger = Math.Max(i0, i1);
	        var edge = (smaller, larger);

	        if(edgeLookup.TryGetValue(edge, out uint midpoint)) {
	            return midpoint;
	        }

	        var v0 = oVertices[i0];
	        var v1 = oVertices[i1];
	        var midpointVertex = Vector3.Normalize((v0 + v1) / 2f);

	        midpoint = (uint) vertices.Count;
	        vertices.Add(midpointVertex);
	        edgeLookup.Add(edge, midpoint);

	        return midpoint;
	    }
	}
}
