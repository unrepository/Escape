using System.Numerics;
using Silk.NET.Maths;

namespace Eclair.Simulation {
	
	public static class RayCast {
		
		public static Vector3 ToHorizontalPlane(
			Vector2 mousePosition,
			Vector2D<int> windowSize,
			Vector3 cameraPosition,
			Matrix4x4 inverseViewMatrix,
			Matrix4x4 inverseProjectionMatrix,
			float planeY = 0
		) {
			var ndcMousePos = new Vector3(
				(2.0f * mousePosition.X) / windowSize.X - 1.0f,
				1.0f - (2.0f * mousePosition.Y) / windowSize.Y,
				-1.0f
			);

			// var mouseDir = new Vector4(ndcMousePos.X, ndcMousePos.Y, ndcMousePos.Z, 1.0f);
			// mouseDir = Vector4.Transform(mouseDir, inverseProjectionMatrix);
			// mouseDir.Z = -1.0f;
			// mouseDir.W = 0.0f;
			//
			// mouseDir = Vector4.Transform(mouseDir, inverseViewMatrix);
			// mouseDir = Vector4.Normalize(mouseDir);
			
			var viewCoords = Vector4.Transform(new Vector4(ndcMousePos.X, ndcMousePos.Y, ndcMousePos.Z, 1), inverseProjectionMatrix);
			
			var rayDirectionView = new Vector3(viewCoords.X, viewCoords.Y, viewCoords.Z) / viewCoords.W;
			var rayCoordsWorld = Vector4.Transform(new Vector4(rayDirectionView, 0.0f), inverseViewMatrix);
			var rayDirectionWorld = Vector3.Normalize(new Vector3(rayCoordsWorld.X, rayCoordsWorld.Y, rayCoordsWorld.Z));
			
			var t = (planeY - cameraPosition.Y) / rayDirectionWorld.Y;
			var x = cameraPosition.X + t * rayDirectionWorld.X;
			var z = cameraPosition.Z + t * rayDirectionWorld.Z;
			
			return new Vector3(x, planeY, z);
		}
	}
}
