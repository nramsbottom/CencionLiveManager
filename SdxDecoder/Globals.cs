using System;

namespace Cencion.SwitchDecoder
{
	public enum CallDirection : short
	{
		Outbound = 0,
		Inbound = 1
	}

} // end namespace


namespace Cencion.SwitchDecoder.Sdx
{
	public delegate void MessageReceivedEventHandler ( object sender, MessageReceivedEventArgs e);
	public delegate void DataReceivedEventHandler( object sender, DataReceivedEventArgs e );
	public delegate void DisconnectedEventHandler ( object sender, DisconnectedEventArgs e );

	public enum DisconnectedReason
	{
		Unknown,
		Shutdown
	}

} // end namespace

