namespace Ogui.UI {
	/// <summary>
	/// A Manager is basically a Component that can be instatiated and added to a Window.
	/// The proposed purpose of this class is to make division of gui logic more convenient.
	/// </summary>
	public class Manager : Component {
		/// <summary>
		/// The parent window of this manager instance.
		/// </summary>
		protected internal Window ParentWindow { get; internal set; }
	}
}