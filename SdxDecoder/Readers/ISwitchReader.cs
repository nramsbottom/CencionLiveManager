using System;

namespace Cencion.SwitchDecoder.Sdx
{
	/// <summary>
	/// Defines the interface for a generic reader object.
	/// </summary>
	public interface ISwitchReader
	{
		void StartReading();

		event DataReceivedEventHandler DataReceived; 
		event DisconnectedEventHandler Disconnected; 

	} // end interface

} // end namespace
