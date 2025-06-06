using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.Planetes;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Drawing.OpenGL;
using GENESIS.PresentationFramework.Extensions;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;
using Window = GENESIS.GPU.Window;

Silk.NET.Windowing.Window.PrioritizeGlfw();

var platformOptions = new GLPlatform.Options();
platformOptions.ParseCommandLine(args);

var platform = new GLPlatform(platformOptions);
var windowOptions = WindowOptions.Default;

var monitorCenter = Monitor.GetMainMonitor(null).Bounds.Center;
					
windowOptions.Position = new Vector2D<int>(
	monitorCenter.X - windowOptions.Size.X / 2,
	monitorCenter.Y - windowOptions.Size.Y / 2
);

var window = Window.Create(platform, windowOptions);

platform.Initialize();
window.Initialize();

var test = new TestScene(new Painter {
	XY = new GLPainter2D(),
	XYZ = new GLPainter3D()
});
window.PushScene(test);

var vertexShader = IShader.Create(platform, ShaderType.VertexShader,
	"""
	#version 430 core
	
	struct Vertex {
	    vec3 position;
	    vec4 color;
	    vec3 normal;
	};
	
	layout(std430, binding = 0) readonly buffer VertexBuffer {
	    Vertex vertices[];
	};
	
	out vec4 vColor;
	
	void main() {
	    gl_Position = vec4(vertices[gl_VertexID].position, 1.0);
	    vColor = vertices[gl_VertexID].color;
	}
	""");

var fragShader = IShader.Create(platform, ShaderType.FragmentShader,
	"""
	#version 430 core
	
	in vec4 vColor;
	out vec4 FragColor;
	
	void main() {
	    FragColor = vColor;
	}
	""");

Vertex[] vertices = new Vertex[] {
	new Vertex { Position = new Vector3(-0.5f, -0.5f, 0), Color = new Vector4(1f, 0f, 0f, 1) },
	new Vertex { Position = new Vector3( 0.5f, -0.5f, 0), Color = new Vector4(0f, 1f, 0f, 1) },
	new Vertex { Position = new Vector3( 0.0f,  0.5f, 0), Color = new Vector4(0f, 0f, 1f, 1) },
};

ShaderArrayData<Vertex> shaderData;

unsafe {
	shaderData = new() {
		Binding = 0,
		Data = vertices,
		Size = (uint) (vertices.Length * sizeof(Vertex))
	};
}

vertexShader.Compile();
fragShader.Compile();

var program = IShaderProgram.Create(platform, fragShader, vertexShader);
program.Build();

vertexShader.PushData(shaderData);

while(!window.Base.IsClosing) {
	window.RenderFrame(delta => {
		program.Bind();
		platform.API.DrawArrays(GLEnum.Triangles, 0, 3);
	});
}

