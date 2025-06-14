using System.Diagnostics;
using System.Numerics;
using GENESIS.GPU;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Drawing {
	
	public static class Models {

		public static readonly Model Cube = new() {
			Name = "cube",
			Meshes = [
				new() {
					Vertices = [
						new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
						new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
						new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
						new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
						new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(0, 1) },
						new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(0, 0) },
						new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 0) },
						new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 1) },
						new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(0, 1) },
						new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(0, 0) },
						new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(1, 0) },
						new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(1, 1) },
						new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 1) },
						new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 0) },
						new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(1, 0) },
						new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(1, 1) },
						new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 0) },
						new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
						new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
						new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 0) },
						new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
						new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 1) },
						new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 1) },
						new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
					],
					Indices = [
						0, 1, 2, 0, 2, 3,
						4, 5, 6, 4, 6, 7,
						8, 9,10, 8,10,11,
						12, 13, 14, 12, 14, 15,
						16, 17, 18, 16, 18, 19,
						20, 21, 22, 20, 22, 23
					]
				}
			]
		};

		public static readonly Model Quad = new() {
			Name = "quad",
			Meshes = [
				new() {
					Vertices = [
						new() { Position = new(-1, -1, 0) },
						new() { Position = new(1, -1, 0) },
						new() { Position = new(-1, 1, 0) },
						new() { Position = new(1, 1, 0) },
					],
					Indices = [
						0, 1, 2,
						2, 1, 3
					]
				}
			]
		};

		public static Model CircleOutline(float radius, float thickness, int segments) {
			var vertices = new List<Vertex>();
			var indices = new List<uint>();

			float innerRadius = radius - thickness / 2;
			float outerRadius = radius + thickness / 2;

			float angle = 2 * MathF.PI / segments;

			for(int i = 0; i < segments; i++) {
				float angle0 = i * angle;
				float angle1 = (i + 1) * angle;

				float cos0 = MathF.Cos(angle0), sin0 = MathF.Sin(angle0);
				float cos1 = MathF.Cos(angle1), sin1 = MathF.Sin(angle1);

				var outer0 = new Vertex {
					Position = new Vector3(outerRadius * cos0, 0, outerRadius * sin0)
				};
				
				var outer1 = new Vertex {
					Position = new Vector3(outerRadius * cos1, 0, outerRadius * sin1)
				};
				
				var inner0 = new Vertex {
					Position = new Vector3(innerRadius * cos0, 0, innerRadius * sin0)
				};
				
				var inner1 = new Vertex {
					Position = new Vector3(innerRadius * cos1, 0, innerRadius * sin1)
				};
				
				// triangle 1
				vertices.Add(inner0);
				vertices.Add(outer0);
				vertices.Add(outer1);
				
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				
				// triangle 2
				vertices.Add(inner0);
				vertices.Add(outer1);
				vertices.Add(inner1);
				
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
			}

			return new() {
				Meshes = [
					new() {
						Vertices = vertices.ToArray(),
						Indices = indices.ToArray()
					}
				]
			};
		}
		
		public static Model EllipseOutline(float rMin, float rMax, float thickness, int segments) {
			var vertices = new List<Vertex>();
			var indices = new List<uint>();

			float majorAxis = MathF.Max(rMin, rMax);
			float minorAxis = MathF.Min(rMin, rMax);

			float innerA = majorAxis - thickness / 2;
			float outerA = majorAxis + thickness / 2;
			float innerB = minorAxis - thickness / 2;
			float outerB = minorAxis + thickness / 2;

			float angle = 2 * MathF.PI / segments;

			for(int i = 0; i < segments; i++) {
				float angle0 = i * angle;
				float angle1 = (i + 1) * angle;

				float cos0 = MathF.Cos(angle0), sin0 = MathF.Sin(angle0);
				float cos1 = MathF.Cos(angle1), sin1 = MathF.Sin(angle1);

				var outer0 = new Vertex {
					Position = new Vector3(outerA * cos0, 0, outerB * sin0)
				};
				
				var outer1 = new Vertex {
					Position = new Vector3(outerA * cos1, 0, outerB * sin1)
				};
				
				var inner0 = new Vertex {
					Position = new Vector3(innerA * cos0, 0, innerB * sin0)
				};
				
				var inner1 = new Vertex {
					Position = new Vector3(innerA * cos1, 0, innerB * sin1)
				};
				
				// triangle 1
				vertices.Add(inner0);
				vertices.Add(outer0);
				vertices.Add(outer1);
				
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				
				// triangle 2
				vertices.Add(inner0);
				vertices.Add(outer1);
				vertices.Add(inner1);
				
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
			}

			return new() {
				Meshes = [
					new() {
						Vertices = vertices.ToArray(),
						Indices = indices.ToArray()
					}
				]
			};
		}

		public static Model? Curve(IList<Vector3> points, float width) {
			if(points.Count < 2) return null;

			int pCount = points.Count;
			if(points.Count % 2 == 1) pCount--;
			
			var vertices = new List<Vertex>();
			var indices = new List<uint>();

			for(int i = 0; i < pCount - 1; i++) {
				var p0 = points[i];
				var p1 = points[i + 1];

				var segment = p1 - p0;
				
				if(segment.Length() < 0.001f) continue;

				var direction = Vector3.Normalize(segment);
				var right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, direction));

				if(right.Length() < 0.001f) right = Vector3.UnitX;

				var v0 = new Vertex {
					Position = p0 - right * width * 0.5f,
					//Normal = Vector3.Cross(direction, right)
				};
			
				var v1 = new Vertex {
					Position = p0 + right * width * 0.5f,
					//Normal = Vector3.Cross(direction, right)
				};
				
				vertices.Add(v0);
				vertices.Add(v1);
				
				indices.Add((uint) indices.Count);
				indices.Add((uint) indices.Count);
			}

			return new() {
				Meshes = [
					new() {
						Vertices = vertices.ToArray(),
						Indices = indices.ToArray()
					}
				]
			};
		}
	}
}
