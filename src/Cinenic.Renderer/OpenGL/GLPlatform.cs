using System.Diagnostics;
using System.Text;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Cinenic.Renderer.OpenGL {
	
	public class GLPlatform : IPlatform {

		public Thread PlatformThread { get; set; }
		public bool IsInitialized { get; set; }
		
		public Options CurrentOptions { get; }

		public GL API { get; protected set; }

		public GLDevice? PrimaryDevice {
			set => throw new NotSupportedException();
			get => throw new NotSupportedException();
		}

		internal static GL? _sharedApi { get; set; }
		internal static IGLContext? _sharedContext { get; set; }
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GLPlatform(PlatformOptions? options = null) {
			CurrentOptions = options as Options ?? new Options();
		}

		public unsafe void Initialize() {
			Debug.Assert(!IsInitialized);
			Debug.Assert(_sharedApi is not null && _sharedContext is not null, "A Window must be created first"); // TODO

			PlatformThread = Thread.CurrentThread;
			API = _sharedApi;

			if(CurrentOptions.Debug) {
				API.Enable(EnableCap.DebugOutput);
				API.Enable(EnableCap.DebugOutputSynchronous);
				
				API.DebugMessageCallback(DebugPrint, null);
				API.DebugMessageControl(
					DebugSource.DontCare,
					DebugType.DontCare,
					DebugSeverity.DontCare,
					0, null,
					true
				);
				
				_logger.Info("Debugging enabled");
			}
			
			IsInitialized = true;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
		
		private unsafe static void DebugPrint(GLEnum source,
		                                        GLEnum type,
		                                        int id,
		                                        GLEnum severity,
		                                        int length,
		                                        nint message,
		                                        nint param) {

			var msg = new StringBuilder("[OpenGL] ");
			msg.Append(id);
			msg.Append('\n');
			msg.Append('\t');

			switch(severity) {
				case GLEnum.DebugSeverityHigh:
					msg.Append("(HIGH)");
					break;
				case GLEnum.DebugSeverityMedium:
					msg.Append("(MEDIUM)");
					break;
				case GLEnum.DebugSeverityLow:
					msg.Append("(LOW)");
					break;
				case GLEnum.DebugSeverityNotification:
					msg.Append("(NOTIFY)");
					break;
			}

			msg.Append(' ');

			switch(type) {
				case GLEnum.DebugTypeError:
					msg.Append("ERROR");
					break;
				case GLEnum.DebugTypeDeprecatedBehavior:
					msg.Append("Deprecated");
					break;
				case GLEnum.DebugTypeUndefinedBehavior:
					msg.Append("Undefined");
					break;
				case GLEnum.DebugTypePortability:
					msg.Append("Portability");
					break;
				case GLEnum.DebugTypePerformance:
					msg.Append("Performance");
					break;
				case GLEnum.DebugTypeMarker:
					msg.Append("Marker");
					break;
				case GLEnum.DebugTypePushGroup:
					msg.Append("Push Group");
					break;
				case GLEnum.DebugTypePopGroup:
					msg.Append("Pop Group");
					break;
				case GLEnum.DebugTypeOther:
					msg.Append("Other");
					break;
			}

			msg.Append('\n');
			msg.Append("\tSource: ");

			switch(source) {
				case GLEnum.DebugSourceApi:
					msg.Append("API");
					break;
				case GLEnum.DebugSourceWindowSystem:
					msg.Append("Window System");
					break;
				case GLEnum.ShaderCompiler:
					msg.Append("Shader Compiler");
					break;
				case GLEnum.DebugSourceThirdParty:
					msg.Append("Third Party");
					break;
				case GLEnum.DebugSourceApplication:
					msg.Append("Application");
					break;
				case GLEnum.DebugSourceOther:
					msg.Append("Other");
					break;
			}

			msg.Append('\n');
			msg.Append("\tMessage: ");

			var m = new string((sbyte*) message, 0, length, Encoding.UTF8);
			msg.Append(m);
			
			_logger.Debug(msg.ToString());
		}
		
		public class Options : PlatformOptions {

			public bool Debug { get; set; } = false;
			
			public override void ParseCommandLine(string[] args) {
				if(args.Contains("--debug")) Debug = true;
			}
		}
	}
}
