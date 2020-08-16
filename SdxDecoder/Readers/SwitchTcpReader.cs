using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Cencion.SwitchDecoder.Sdx
{

	/// <summary>
	/// Summary description for SwitchTcpReader.
	/// </summary>
	/// 
	public class TcpReader : ISwitchReader
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

		/// <summary>
		/// Begins aquiring data from the data source.
		/// </summary>
		public void StartReading()
		{
		
			this._run = true;
			
			this.recvThread = new Thread ( new ThreadStart( this.BeginRecv ) );
			this.recvThread.Start();
		}

		/// <summary>
		/// Stops reading data from the datasource *NOT WORKING*
		/// </summary>
		public void StopReading()
		{
			throw new NotImplementedException("This function is incomplete.");

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

		/// <summary>
		/// Begins reading data from the specified host.
		/// </summary>
		private void BeginRecv()
		{
		
			NetworkStream ns;
			StreamReader rdr;

			// create a new tcp connection
			this.connection = new TcpClient ( this.swHost, this.swPort );

			// get the network stream
			ns = this.connection.GetStream();

			// create a new StreamReader on the NetworkStream to allow data to be... read... from the network
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

			// read forever - a nasty hack. 
			while (true) // TODO: needs to be redone
			{

				string line = rdr.ReadLine();

				// raise event
				if ( this.DataReceived != null )
					this.DataReceived ( this, new DataReceivedEventArgs(line) );

			}

			//Console.WriteLine("ENDED");

			//this.Shutdown();
		}

		/// <summary>
		/// Stop reading data from the data source and release resources.
		/// </summary>
		private void Shutdown()
		{

			/****
			 * 
			 * NOT WORKING
			 *
			 ***/

			DisconnectedReason disconnectReason;

			// set the reason based on the status of the running variable.
			if ( this._run )
				disconnectReason = DisconnectedReason.Unknown; // unexpected shutdown or the remote end stopped sending data
			else
				disconnectReason = DisconnectedReason.Shutdown; // shutdown was requested

			if ( this.Disconnected != null )
				this.Disconnected( this, new DisconnectedEventArgs ( disconnectReason ) );
			
		}

	} // end class
} // end namspace
