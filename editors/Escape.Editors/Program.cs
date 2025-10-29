using Spectre.Console.Cli;

namespace Escape.Editors {

	public class Program {

		public static int Main(string[] args) {
			var app = new CommandApp();
			
			app.Configure(config => {
				config.AddBranch("editor", editor => {
					editor.AddCommand<ResourceEditor.Command>("resource");
				});
			});

			return app.Run(args);
		}
	}
}
