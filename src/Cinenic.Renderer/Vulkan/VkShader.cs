using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.OpenGL;
using NLog;
using Silk.NET.Core;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkShader : Shader.Shader {

		internal ShaderModule Module { get; private set; }
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;

		public VkShader(VkPlatform platform, ShaderType type, string code) : base(platform, type, code) {
			_platform = platform;
		}

		public unsafe override uint Compile() {
			Debug.Assert(Handle == 0);

			var codePtr = Marshal.StringToHGlobalAuto(Code);
			Debug.Assert(codePtr != 0);

			var moduleInfo = new ShaderModuleCreateInfo {
				SType = StructureType.ShaderModuleCreateInfo,
				CodeSize = (uint) Code.Length,
				PCode = (uint*) codePtr
			};
			
			Result result;
			if(
				(result = _platform.API.CreateShaderModule(_platform.PrimaryDevice.Logical, &moduleInfo, null, out var module))
				!= Result.Success
			) {
				throw new PlatformException($"Could not create shader module: {result}");
			}

			Module = module;
			Handle = module.Handle;
		}
		
		public override void Dispose() {
			throw new NotImplementedException();
		}
	}
}
