using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;

namespace GENESIS.PresentationFramework.Camera {

	public abstract class CameraBase {

		public Matrix4x4 ProjectionMatrix { get; protected set; }
		public Matrix4x4 ViewMatrix { get; protected set; }

		public Matrix4x4 InverseProjectionMatrix { get; protected set; }
		public Matrix4x4 InverseViewMatrix { get; protected set; }
		
		public float FieldOfView {
			get => field;
			set {
				field = value;
				RecalculateProjectionMatrix();
			}
		}
		
		protected float Width { get; private set; }
		protected float Height { get; private set; }

		protected ShaderData<CameraData> ShaderData;

		protected CameraBase(Window window, IShader shader) {
			Width = window.Base.FramebufferSize.X;
			Height = window.Base.FramebufferSize.Y;
			
			RecalculateProjectionMatrix();
			RecalculateViewMatrix();

			window.Base.FramebufferResize += size => {
				Width = size.X;
				Height = size.Y;
				
				RecalculateProjectionMatrix();
			};
			
			ShaderData = new();
			ShaderData.Data = new();
			
			shader.PushData(ShaderData);
		}

		protected abstract void RecalculateProjectionMatrix();
		protected abstract void RecalculateViewMatrix();

		public abstract void Update();
	}
}
