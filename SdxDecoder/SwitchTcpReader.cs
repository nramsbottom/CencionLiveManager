using System;

using System.IO;

using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace Cencion.SwitchDecoder.Sdx
{

	
	public enum DisconnectedReason
	{
		Unknown,
		Shutdown
	}

	/// <summary>
	/// Summary description for DataReceivedEventArgs.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		private string _data;

		public string Data
		{
			get { return this._data; } 
		}

		public DataReceivedEventArgs ( string data )
		{
			this._data = data;
		}

	}

	/// <summary>
	/// Summary description for SwitchDisconnectedEventArgs.
	/// </summary>
	public class DisconnectedEventArgs : EventArgs
	{

		private DisconnectedReason _reason;

		public DisconnectedEventArgs ( DisconnectedReason reason )
		{
			this._reason = reason;
		}

		public DisconnectedReason Reason
		{
			get { return this._reason; }
		}
	}

	public delegate void DataReceivedEventHandler( object sender, DataReceivedEventArgs e );
	public delegate void DisconnectedEventHandler ( object sender, DisconnectedEventArgs e );

	/// <summary>
	/// Summary description for SwitchTcpReader.
	/// </summary>
	/// 
	public class TcpReader
	{

		private string swHost;
		private short swPort;

		public event DataReceivedEventHandler DataReceived; 
		public event DisconnectedEventHandler Disconnected; 

		private TcpClient connection;
		private Thread recvThread;
		private bool _run;

		public TcpReader( string host, short port )
		{
			//
			//
			if ( host == null)
				throw new ArgumentNullException( "host" );
			
			if ( host.Length == 0 )
				throw new ArgumentException( "host", "No host specified" );

			if ( port <= 0 )
				throw new ArgumentNullException( "port", "Invalid port number" );

			this.swHost = host;
			this.swPort = port;

		}

		public void StartReading()
		{
		
			this._run = true;
			
			this.recvThread = new Thread ( new ThreadStart( this.BeginRecv ) );
			this.recvThread.Start();
		}

		public void StopReading()
		{
			// TODO: Possible threading issues because a different thread will be accessing this
			this._run = false;

			try
			{
				this.recvThread.Abort();
				// Thread.CurrentThread.Abort(); // CHECK: is this aborting the process or the worker thread?
			}
			catch (ThreadAbortException ex)
			{
				// dont panic!! this exception is expected
			}
			finally
			{
				Shutdown();
			}

		}

		private void BeginRecv()
		{
		
			NetworkStream ns;
			StreamReader rdr;

			this.connection = new TcpClient ( this.swHost, this.swPort );

			ns = this.connection.GetStream();
			rdr = new StreamReader ( ns );

			// the commented code below is causing problems. 'half' works
			/*
			// check here to allow graceful shutdown
			while (this._run)
			{
				// only begin the blocking readline method if there is data to be read
				if ( rdr.Peek() > -1 )
				{
					string line = rdr.ReadLine();

					// raise event
					if ( this.DataReceived != null )
						this.DataReceived ( this, new DataReceivedEventArgs(line) );

				}
				else
					Thread.Sleep(1); // yield cpu to other processes

			}

			rdr.Close();
			ns.Close();
			*/

			while (true)
			{
				string line = rdr.ReadLine();

				// raise event
				if ( this.DataReceived != null )
					this.DataReceived ( this, new DataReceivedEventArgs(line) );

			}

			Console.WriteLine("ENDED");

			//this.Shutdown();
		}

		private void Shutdown()
		{

			
			DisconnectedReason disconnectReason;

			if ( this._run )
				disconnectReason = DisconnectedReason.Unknown; // unexpected shutdown or the remote end stopped sending data
			else
				disconnectReason = DisconnectedReason.Shutdown; // shutdown was requested

			if ( this.Disconnected != null )
				this.Disconnected( this, new DisconnectedEventArgs ( disconnectReason ) );
			
		}


	}
}
