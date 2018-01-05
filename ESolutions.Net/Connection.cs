
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ESolutions.Net
{
	/// <summary>
	/// Objects of this class represent a socket connection between a local and a far ip end point.
	/// Data can either be sent or received by this class. Received tempDataBuffer is communicated to 
	/// other classes by using the ReceivedData event. Mention that this event is always executed in
	/// a separate thread. So don't forget to use Invoke when using objects of this class on
	/// a WinForm.
	/// </summary>
	public class Connection
	{
		#region socket
		/// <summary>
		/// This socket is used to receive and send tempDataBuffer to the connected far end point.
		/// </summary>
		private Socket socket;
		#endregion

		#region IsUsable
		/// <summary>
		/// Get a value indicating wether the connection can be used for transmissions or not.
		/// </summary>
		public Boolean IsUsable
		{
			get
			{
				return socket.Connected;
			}
		}
		#endregion

		#region FarEndPoint
		/// <summary>
		/// Get an IPEndPaint representing the far client's IP address of this connection.
		/// </summary>
		public IPEndPoint FarEndPoint
		{
			get
			{
				return socket.RemoteEndPoint as IPEndPoint;
			}
		}
		#endregion

		#region LocalEndPoint
		/// <summary>
		/// Gets an IPEndPoint reference, representing the local end point of the connection.
		/// </summary>
		public IPEndPoint LocalEndPoint
		{
			get
			{
				return socket.LocalEndPoint as IPEndPoint;
			}
		}
		#endregion

		#region Close
		/// <summary>
		/// Attemps to close the connection emmidiatly
		/// </summary>
		public void Close ()
		{
			try
			{
				this.socket.Shutdown (SocketShutdown.Both);
				this.socket.Close ();
			}
			catch
			{
			}
			finally
			{
				this.OnAfterClose ();
			}
		}
		#endregion

		#region Connection
		/// <summary>
		/// Internal constructor used to establish connections which socket already existst. 
		/// Usually this is used a far end point connects to a listening socket. In this case
		/// the tempDataBuffer transport socket is already known.
		/// </summary>
		/// <param name="newSocket">The socket that is used to transport tempDataBuffer.</param>
		/// <param name="waitForData">Indicates wether the connection goes into blocking mode awaiting tempDataBuffer or not.</param>
		internal Connection (
			Socket newSocket, 
			bool waitForData)
		{
			socket = newSocket;

			if (waitForData)
			{
				StartReceiving ();
			}
		}
		#endregion

		#region Connection
		/// <summary>
		/// Internal constructor used to establish connections which to far end point of which only the
		/// ip address and port is known. Usually this is used to establish a new sending connection to 
		/// a far end point.
		/// </summary>
		/// <param name="farIpEndPoint">The ip address and port of the far ip end point to connect to.</param>
		/// <param name="waitForData">Indicates wether the connection goes into blocking mode awaiting tempDataBuffer or not.</param>
		internal Connection (
			IPEndPoint farIpEndPoint, 
			bool waitForData)
		{
			socket = new Socket (
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp);

			socket.Connect (farIpEndPoint);

			if (waitForData)
			{
				StartReceiving ();
			}
		}
		#endregion

		#region ~Connection
		/// <summary>
		/// Cleans up all all resources and closes the internal socket if its still open.
		/// </summary>
		~Connection ()
		{
			this.Close ();

			if (socket.Connected)
			{
				Console.WriteLine (
					"Winsock error: " + 
					Convert.ToString (System.Runtime.InteropServices.Marshal.GetLastWin32Error ()));

				System.Diagnostics.Debug.Write (
					"Winsock error: " + 
					Convert.ToString (System.Runtime.InteropServices.Marshal.GetLastWin32Error ()));
			}
		}
		#endregion

		#region Send
		/// <summary>
		/// Sends tempDataBuffer to the far end point of the connection.
		/// </summary>
		/// <param name="data">The package to be send to the remote end point of the connection.</param>
		/// <param name="waitForReply">Indicates wether the method waits for a result or not. If this flag is set to true the MemoryStream returned by the method contains the result of the far end point. If this flag is set to false the returned value is null.</param>
		/// <returns>A MemoryStream with the answer of the far end point if waitForReply was set to true otherwise null.</returns>
		/// <remarks>
		/// Call this method to send package to the connected far end point. If the waitForReply flag
		/// was set to true the MemoryStream returned by the method contains the answer of the far
		/// end point  (if any is sent). All tempDataBuffer are converted into a UTF-8 encodes byte 
		/// array for the transmission.
		/// </remarks>
		public Package Send (
			Package data,
			bool waitForReply)
		{
			Package result = null;

			data.RemoteIsWaiting = waitForReply;
		
			socket.Send (data.ToTranmissionString ());
			
			if (waitForReply)
			{
				Int32 lengthOfTransmisson = GetLengthOfTransmission();
				byte[] transmissionData = ReceiveTransmission(lengthOfTransmisson);

				result = new Package ();
				result.LoadXml(transmissionData);
			}

			return result;
		}
		#endregion

		#region StartReceiving
		/// <summary>
		/// Starts an infinite thread that wait for tempDataBuffer beeing sent by the far end point to the
		/// local end point.
		/// </summary>
		private void StartReceiving ()
		{
			System.Threading.Thread receivingThread = new System.Threading.Thread (
				new System.Threading.ThreadStart (Receiving));

			receivingThread.Start ();
		}
		#endregion

		#region Receiving
		/// <summary>
		/// Receives package from connected ip end points.
		/// </summary>
		/// <remarks>
		/// This method uses a sockets receives method to block until package is send to the socket 
		/// by a connected end point. The first ten bytes must include a string telling the
		/// lenght of the following package packet. The following datapacket must have the XML-Format
		/// defined by the Package class. 
		/// </remarks>
		/// <seealso cref="Package"/>
		private void Receiving ()
		{
			try
			{
				while (true)
				{
					Int32 lengthOfTransmisson = GetLengthOfTransmission ();
					byte[] transmissionData = ReceiveTransmission (lengthOfTransmisson);
					Package newPackage = new Package ();
					newPackage.LoadXml (transmissionData);
					OnPackageReceived (newPackage);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine (ex.Message);
			}
			finally
			{
				this.Close ();
			}
		}
		#endregion

		#region GetLengthOfTransmission
		/// <summary>
		/// Receives the first ten bytes of a transmission which contains the length
		/// of the rest of the transmission.
		/// </summary>
		/// <returns>Returns the length of the transmission that follows</returns>
		private Int32 GetLengthOfTransmission ( )
		{
			byte[] dataBuffer = new byte[10];

			this.socket.Receive (
				dataBuffer,
				0,
				10,
				SocketFlags.None);

			String lenghtInformation = UTF8Encoding.UTF8.GetString (dataBuffer);

			return Convert.ToInt32 (lenghtInformation);
		}
		#endregion

		#region ReceiveTransmission
		/// <summary>
		/// Receives the passed amount of bytes from the local socket.
		/// </summary>
		private byte[] ReceiveTransmission (Int32 expectedLength)
		{
			byte[] result = new byte[expectedLength];

			Int32 receivedDataInTransmission = 0;
			Int32 receivedDataInLoop = 0;
			Int32 receiveDataInLoop = 0;
			byte[] dataBuffer = new byte[256];

			while (receivedDataInTransmission < expectedLength)
			{
				if (expectedLength - receivedDataInTransmission < 256)
				{
					receiveDataInLoop = expectedLength - receivedDataInTransmission;
				}
				else
				{
					receiveDataInLoop = 256;
				}

				receivedDataInLoop = this.socket.Receive (
					dataBuffer,
					0,
					receiveDataInLoop,
					SocketFlags.None);

				Array.Copy (
					dataBuffer,
					0,
					result,
					receivedDataInTransmission,
					receivedDataInLoop);

				receivedDataInTransmission += receivedDataInLoop;
			}
			
			return result;
		}
		#endregion

		#region OnPackageReceived
		/// <summary>
		/// Fires the ReceivedData event.
		/// </summary>
		/// <param name="package">The received Package</param>
		private void OnPackageReceived (
			Package package)
		{
			if (PackageReceived != null)
			{
				PackageReceivedEventArgs e = new PackageReceivedEventArgs (
					this,
					package);

				PackageReceived (this, e);
			}
		}
		#endregion

		#region PackageReceived
		/// <summary>
		/// Is fired each time the connection received a package.
		/// </summary>
		public event PackageReceivedEventHandler PackageReceived;
		#endregion

		#region OnAfterClose
		/// <summary>
		/// Fires the AfterClose event
		/// </summary>
		protected void OnAfterClose ( )
		{
			if (AfterClose != null)
			{
				AfterClose (
					this,
					new EventArgs ());
			}
		}
		#endregion

		#region AfterClose
		/// <summary>
		/// Is fired after the current connection was closed.
		/// </summary>
		public event EventHandler AfterClose;
		#endregion
	}
}
