using System.Runtime.CompilerServices;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public static class VkHelpers {

		private static Logger _logger = LogManager.GetCurrentClassLogger();
		
		public static bool VkCheck(
			Result result, string errorMessage,
			bool fatal = true,
			[CallerFilePath] string file = "",
			[CallerLineNumber] int line = 0,
			[CallerMemberName] string member = ""
		) {
			if(result != Result.Success) {
				string msg = $"VkCheck failed @ {file}:{line}/{member} (result == {result}): {errorMessage}";
				
				if(!fatal) {
					_logger.Error(msg);
					return false;
				}
				
				throw new PlatformException(msg);
			}

			return true;
		}
	}
}
