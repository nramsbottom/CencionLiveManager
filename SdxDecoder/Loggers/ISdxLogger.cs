using System;

namespace Cencion.SwitchDecoder.Sdx.Loggers
{
	/// <summary>
	/// Common interface for event loggers.
	/// </summary>
	public interface ISdxLogger
	{
		void OnMessageReceived(object sender, MessageReceivedEventArgs e);
	} // end interface

} // end namespace
