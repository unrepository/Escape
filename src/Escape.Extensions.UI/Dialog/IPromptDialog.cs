namespace Escape.Extensions.UI.Dialog {
	
	public interface IPromptDialog<TResult> {

		public bool IsOpen { get; set; }
		public TResult? Result { get; }

		public bool Prompt(bool popup = true);
	}
}
