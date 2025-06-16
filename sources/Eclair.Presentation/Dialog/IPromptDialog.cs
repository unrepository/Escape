namespace Eclair.Presentation.Dialog {
	
	public interface IPromptDialog<TResult> {

		public bool IsOpen { get; }
		public TResult? Result { get; }

		public bool Prompt(bool popup = true);
	}
}
