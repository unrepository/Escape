using System.Numerics;

namespace Eclair.Extensions.CSharp {
	
	public static class Matrix4x4Extensions {
		
		public static Vector3 PositiveX(this Matrix4x4 matrix, ref Vector3 dir) {
			dir.X = matrix.M11;
			dir.Y = matrix.M21;
			dir.Z = matrix.M31;

			Vector3.Normalize(dir);
			return dir;
		}

		public static Vector3 PositiveY(this Matrix4x4 matrix, ref Vector3 dir) {
			dir.X = matrix.M12;
			dir.Y = matrix.M22;
			dir.Z = matrix.M32;

			Vector3.Normalize(dir);
			return dir;
		}
		
		public static Vector3 PositiveZ(this Matrix4x4 matrix, ref Vector3 dir) {
			dir.X = matrix.M13;
			dir.Y = matrix.M23;
			dir.Z = matrix.M33;

			Vector3.Normalize(dir);
			return dir;
		}
		
		public static Vector3 PositiveX(this Matrix4x4 matrix) {
			return Vector3.Normalize(new(matrix.M11, matrix.M21, matrix.M31));
		}

		public static Vector3 PositiveY(this Matrix4x4 matrix) {
			return Vector3.Normalize(new(matrix.M12, matrix.M22, matrix.M32));
		}
		
		public static Vector3 PositiveZ(this Matrix4x4 matrix) {
			return Vector3.Normalize(new(matrix.M13, matrix.M23, matrix.M33));
		}
	}
}
