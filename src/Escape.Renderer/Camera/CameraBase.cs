using System.Numerics;
using Escape.Renderer.Shader;

namespace Escape.Renderer.Camera {

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
		} = 1;
		
		//public CameraData Data { get; protected set; }
		
		public float Width { get; private set; }
		public float Height { get; private set; }

		protected CameraBase(Framebuffer framebuffer)
			: this((int) framebuffer.Size.X, (int) framebuffer.Size.Y)
		{
			framebuffer.Resized += newSize => {
				Width = newSize.X;
				Height = newSize.Y;
				
				RecalculateProjectionMatrix();
			};
		}

		protected CameraBase(Window window)
			: this(window.Size.X, window.Size.Y)
		{
			window.Base.FramebufferResize += newSize => {
				Width = newSize.X;
				Height = newSize.Y;

				RecalculateProjectionMatrix();
			};
		}

		protected CameraBase(int width, int height) {
			Width = width;
			Height = height;
			
			RecalculateProjectionMatrix();
			Update();
		}

		protected abstract void RecalculateProjectionMatrix();
		protected abstract void RecalculateViewMatrix();

		public abstract void Update();
	}
}
