// TODO make into prefab?

using System.Diagnostics;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Components;
using Escape.Extensions.CSharp;
using Escape.Renderer;
using Escape.UnitTypes;
using Silk.NET.Core;
using Silk.NET.Input;
using Escape;

public class OrbitCamera3D : IUpdater {

	public string Id { get; }
	public Entity CameraEntity { get; set; }

	public double Sensitivity { get; set; } = 5.0f;

	public bool AllowZooming { get; set; } = true;
	public bool AllowPanning { get; set; } = true;
	public bool AllowRotating { get; set; } = true;
	
	public Vector3 Target = Vector3.Zero;
	public float Distance { get; set; } = 2.0f;
	
	public IMouse MovementMouse { get; }
	public IKeyboard? MovementKeyboard { get; }

	private Vector2 _lastMousePosition;
	
	public OrbitCamera3D(string id, Entity cameraEntity, Window window) {
		Id = id;
		CameraEntity = cameraEntity;
		
		Debug.Assert(cameraEntity.Has<Transform3D>());
		Debug.Assert(cameraEntity.Has<Camera3D>());

		if(window.Input!.Mice.Count < 1) {
			throw new PlatformException("At least one mice required");
		}

		if(window.Input!.Keyboards.Count < 1) {
			throw new PlatformException("At least one keyboard is required");
		}

		MovementMouse = window.Input.Mice[0];
		MovementKeyboard = window.Input.Keyboards[0];

		MovementMouse.Scroll += (_, scroll) => {
			if(!AllowZooming) return;

			float multiplier = 0.2f;
			if(MovementKeyboard?.IsKeyPressed(Key.ShiftLeft) == true) multiplier = 5;
			if(MovementKeyboard?.IsKeyPressed(Key.ControlLeft) == true) multiplier = 10;
			if(MovementKeyboard?.IsKeyPressed(Key.AltLeft) == true) multiplier = 20;
			
			Distance -= scroll.Y * multiplier;
			Distance = MathF.Max(Distance, 0.1f);
		};
	}

	public void Update(TimeSpan delta) {
		var deltaX = (MovementMouse.Position.X - _lastMousePosition.X) * Sensitivity * delta.TotalSeconds;
		var deltaY = (MovementMouse.Position.Y - _lastMousePosition.Y) * Sensitivity * delta.TotalSeconds;
		_lastMousePosition = MovementMouse.Position;

		ref var t3d = ref CameraEntity.TryGetRef<Transform3D>(out _);
		
		var mouseCaptured = false;

		if(!mouseCaptured) {
			float div = 200.0f;
			
			// rotation
			if(AllowRotating && MovementMouse.IsButtonPressed(MouseButton.Left)) {
				t3d.Yaw -= Rotation<float>.FromDegrees((float) deltaX);
				t3d.Pitch += Rotation<float>.FromDegrees((float) deltaY);
			}

			// translation X/Z
			if(AllowPanning && MovementMouse.IsButtonPressed(MouseButton.Right)) {
				float prevY = Target.Y;

				var c3d = CameraEntity.Get<Camera3D>();
			
				Target -= Vector3.Multiply(c3d.Camera.ViewMatrix.PositiveX(),
					(float) (deltaX * (Sensitivity / div)));
				Target -= Vector3.Multiply(c3d.Camera.ViewMatrix.PositiveZ(),
					(float) (deltaY * (Sensitivity / div)));
			
				Target.Y = prevY;
			}
		
			// translation Y
			if(AllowPanning && MovementMouse.IsButtonPressed(MouseButton.Middle)) {
				Target.Y -= (float) (deltaY * (Sensitivity / div));
			}
		}
		
		t3d.Rotation = Quaternion.CreateFromYawPitchRoll(
			t3d.Yaw.Radians,
			t3d.Pitch.Radians,
			0
		);
		
		var offset = Vector3.Transform(
			new Vector3(0, 0, Distance),
			t3d.Rotation
		);
		
		t3d.Position = Target - offset;
	}
}
