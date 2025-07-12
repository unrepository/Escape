using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.OpenGL;
using NLog;
using Silk.NET.Core;
using Silk.NET.OpenGL;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkShader : Shader.Shader {

		public static readonly Shaderc CompilerAPI = Shaderc.GetApi();
		public static readonly Compiler* Compiler = CompilerAPI.CompilerInitialize();
		
		internal ShaderModule Module { get; private set; }
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;

		public VkShader(VkPlatform platform, Family type, string code) : base(platform, type, code) {
			_platform = platform;
		}

		public unsafe override ulong Compile() {
			Debug.Assert(Handle == 0);
			
			_logger.Debug("Compiling shader");
			_logger.Trace(Code);
			
			var codePtr = Marshal.StringToHGlobalAuto(Code);
			Debug.Assert(codePtr != 0);

			var compileOptions = CompilerAPI.CompileOptionsInitialize();
			CompilerAPI.CompileOptionsSetTargetEnv(compileOptions, TargetEnv.Vulkan, 1);
			
			var compiledShader = CompilerAPI.CompileIntoSpv(
				Compiler,
				(byte*) codePtr,
				(uint) Code.Length,
				Type switch {
					Family.Vertex => ShaderKind.VertexShader,
					Family.Fragment => ShaderKind.FragmentShader,
					Family.Compute => ShaderKind.ComputeShader,
					Family.Geometry => ShaderKind.GeometryShader,
					Family.TessellationControl => ShaderKind.TessControlShader,
					Family.TessellationEvaluation => ShaderKind.TessEvaluationShader
				},
				"shader.shader", // TODO when we switch to a proper resource system
				"main",
				compileOptions
			);

			var status = CompilerAPI.ResultGetCompilationStatus(compiledShader);
			if(status != CompilationStatus.Success) {
				_logger.Fatal(Marshal.PtrToStringAuto((IntPtr) CompilerAPI.ResultGetErrorMessage(compiledShader)) ?? "Unknown error");
				throw new PlatformException($"Shader compilation failed: {status}");
			}
			
			_logger.Debug("Shader compilation status: {Status}", status);

			var moduleInfo = new ShaderModuleCreateInfo {
				SType = StructureType.ShaderModuleCreateInfo,
				CodeSize = (uint) Code.Length,
				PCode = (uint*) CompilerAPI.ResultGetBytes(compiledShader)
			};
			
			Result result;
			if(
				(result = _platform.API.CreateShaderModule(_platform.PrimaryDevice!.Logical, &moduleInfo, null, out var module))
				!= Result.Success
			) {
				throw new PlatformException($"Could not create shader module: {result}");
			}

			Module = module;
			Handle = module.Handle;
			
			_logger.Debug("Successfully created shader module");

			return Handle;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				_platform.API.DestroyShaderModule(_platform.PrimaryDevice!.Logical, Module, null);
			}

			Handle = 0;
		}
	}
}
