using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace ESolutions.Net
{
	/// <summary>
	/// Objects of this class represent a single networkadapter in your environment. 
	/// A networkadapter can send and receive tempDataBuffer on any port. Your application should
	/// only use one instance of the class for each physical networkadapter in your environment.
	/// </summary>
	public class NetworkAdapter
	{
		#region OutgoingConnections
		/// <summary>
		/// A list of all connections that were established using the Connect method of this NetworkAdapter
		/// </summary>
		private List<Connection> outgoingConnections = new List<Connection> ();
		#endregion

		#region incomingConnections
		/// <summary>
		/// A list of all Connections that have been accepted by this networkAdapter for all the ports it is listening to
		/// </summary>
		private List<Connection> incomingConnections = new List<Connection> ();
		#endregion

		#region listeningSockets
		/// <summary>
		/// A list of all sockets and ports to which the networkadapter listens to. All theses sockets accept connections.
		/// </summary>
		private SortedList<Int32, Socket> listeningSockets = new SortedList<Int32, Socket> ();
		#endregion

		#region IpAddress
		/// <summary>
		/// Backing field of IpAddress. IP-Address of the physical network adapter, that is represented by an object of this class
		/// </summary>
		private IPAddress ipAddress;
		#endregion

		#region IpAddress
		/// <summary>
		/// IP-Address of the physical network adapter, that is represented by an object of this class
		/// </summary>
		public IPAddress IpAddress
		{
			get
			{
				return ipAddress;
			}
		}
		#endregion

		#region PortStack
		/// <summary>
		/// This stack is used to passed the desired ports to the listen thread
		/// </summary>
		Stack<int> portStack = new Stack<int> ();
		#endregion

		#region NetworkAdapater
		/// <summary>
		/// Constructor for a new instance of a network adapter class. 
		/// </summary>
		/// <param name="ipAddress">The ip address of the represented network adapter</param>
		public NetworkAdapter (IPAddress ipAddress)
		{
			this.ipAddress = ipAddress;
			Application.ApplicationExit += new EventHandler (Application_ApplicationExit);
		}
		#endregion

		#region Application_ApplicationExit
		/// <summary>
		/// When the application ends all listening and connected sockets are closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Application_ApplicationExit (object sender, EventArgs e)
		{
			CloseAllListeningSockets ();
			CloseAllConnections ();
		}
		#endregion

		#region CloseAllConnection
		/// <summary>
		/// Cloeses all open incoming and outgoing connections.
		/// </summary>
		private void CloseAllConnections ()
		{
			while (this.incomingConnections.Count > 0)
			{
				if (this.incomingConnections[0].IsUsable == false)
				{
					this.incomingConnections.RemoveAt (0);
				}
				else
				{
					this.incomingConnections[0].Close ();
				}
			}

			while (this.outgoingConnections.Count > 0)
			{
				if (this.outgoingConnections[0].IsUsable == false)
				{
					this.outgoingConnections.RemoveAt (0);
				}
				else
				{
					this.outgoingConnections[0].Close ();
				}
			}
		}
		#endregion

		#region CloseAllListeningSockets
		/// <summary>
		/// Closes all listining ports.
		/// </summary>
		private void CloseAllListeningSockets ()
		{
			while (this.listeningSockets.Count > 0)
			{
				this.StopListening (this.listeningSockets.Keys[0]);
			}
		}
		#endregion

		#region StartListening
		/// <summary>
		/// This methods starts a new thread with a socket accepting connection on the specified port
		/// </summary>
		/// <param name="port">Number of the port to which the network adapter should listen</param>
		/// <exception cref="NetworkAdapterException">Thrown when the networkadapter already listens to the port.</exception>
		public void StartListening (int port)
		{
			if (listeningSockets.ContainsKey (port))
			{
				throw new NetworkAdapterException (NetworkAdapterExceptions.NetworkAdapterAlreadyListensToTheSpecifiedPort);
			}

			portStack.Push (port);
			Thread listenThread = new Thread (new ThreadStart (Listen));
			listenThread.Start ();
		}
		#endregion

		#region StopListening
		/// <summary>
		/// Stop the listening socket on the specified port.
		/// </summary>
		/// <param name="port">Number of te port to which shall not be longer listened.</param>
		/// <exception cref="NetworkAdapterException">Thrown when there is no socket listening to the specified port.</exception>
		public void StopListening (int port)
		{
			if (listeningSockets.ContainsKey (port) == false)
			{
				throw new NetworkAdapterException (NetworkAdapterExceptions.NetworkAdapterIsNotListeningToThisPort);
			}

			Socket listeningSocket = this.listeningSockets[port];
			this.listeningSockets.Remove (port);
			listeningSocket.Close ();
		}
		#endregion

		#region Listen
		/// <summary>
		/// Listens to the first port on the portStack on the ip address of the network adapter.
		/// </summary>
		private void Listen ()
		{
			String localIpEndPointReminder = this.ipAddress.ToString ();

			try
			{
				Socket newListeningSocket = new Socket (
					AddressFamily.InterNetwork, 
					SocketType.Stream, 
					ProtocolType.Tcp);

				int port = portStack.Pop ();

				localIpEndPointReminder += ":" + port.ToString ();

				IPEndPoint newIpEndPoint = new IPEndPoint (this.ipAddress, port);
				
				newListeningSocket.Bind (
					newIpEndPoint);

				newListeningSocket.Listen (
					1000);
				
				listeningSockets.Add (
					port, 
					newListeningSocket);
				
				AcceptConnections (
					newListeningSocket);
			}
			catch (Exception ex)
			{
				throw new NetworkAdapterException (
					String.Format (NetworkAdapterExceptions.AcceptingConnectionsFailed, localIpEndPointReminder),
					ex);
			}
		}
		#endregion

		#region AcceptConnections
		/// <summary>
		/// Accepts connections from other network adapters on the listening socket
		/// </summary>
		/// <remarks>
		/// There is an infinite loop that keeps accepting connections until the listening socket is closed. In this case an exception forces the loop to end.
		/// It is important to close all listening sockets using the StopListening method. Otherwise there could remain zombie sockets after closing the application.
		/// </remarks>
		/// <param name="listeningSocket">The socket accepting the new connections.</param>
		private void AcceptConnections (Socket listeningSocket)
		{
			String localEndPointReminder = listeningSocket.LocalEndPoint.ToString ();

			try
			{
				while (true)
				{
					Socket acceptedSocket = listeningSocket.Accept ();

					Connection newConnection = new Connection (
						acceptedSocket, 
						true);

					newConnection.PackageReceived += new PackageReceivedEventHandler (Connection_PackageReceived);
					newConnection.AfterClose += new EventHandler (Connection_AfterClose);

					incomingConnections.Add (
						newConnection);

					OnConnectionAccepted (newConnection);
				}
			}
			catch
			{
				System.Diagnostics.Trace.WriteLine (
					"The socket accepting connections on " + localEndPointReminder + " has been closed.");
			}
		}
		#endregion

		#region Connection_PackageReceived
		/// <summary>
		/// Is called whenever a connection received tempDataBuffer.
		/// </summary>
		/// <param name="sender">Connection object sending the event.</param>
		/// <param name="e">The paramters for the event containing the received tempDataBuffer.</param>
		private void Connection_PackageReceived (
			object sender, 
			PackageReceivedEventArgs e)
		{
			try
			{
				OnPackageReceived (
					sender as Connection, 
					e.Package);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write (ex.Message);
				Console.WriteLine (ex.Message);
			}
		} 
		#endregion

		#region Connection_AfterClose
		/// <summary>
		/// Passes the close event from the connection to the adapter.
		/// </summary>
		private void Connection_AfterClose (
			object sender,
			EventArgs e)
		{
			if (incomingConnections.Contains (sender as Connection))
			{
				incomingConnections.Remove (sender as Connection);
			}
			else
			{
				if (outgoingConnections.Contains (sender as Connection))
				{
					outgoingConnections.Remove (sender as Connection);
				}
			}

			OnAfterConnectionClosed (sender as Connection);
		}
		#endregion

		#region Connect
		/// <summary>
		/// Establishes a new connection to a far end point.
		/// </summary>
		/// <param name="ipEndPoint">The far ip end point.</param>
		/// <returns>An object representing the newly established connection.</returns>
		public Connection Connect (IPEndPoint ipEndPoint)
		{
			Connection newConnection = null;

			newConnection = new Connection (ipEndPoint, false);
			outgoingConnections.Add (newConnection);

			newConnection.AfterClose +=new EventHandler(Connection_AfterClose);

			return newConnection;
		} 
		#endregion

		#region OnPackageReceived
		/// <summary>
		/// Fires the PackageReceived event
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="package"></param>
		protected void OnPackageReceived (
			Connection connection, 
			Package package)
		{
			if (PackageReceived != null)
			{
				PackageReceivedEventArgs e = new PackageReceivedEventArgs (
					connection,
					package);

				PackageReceived (this, e);
			}
		} 
		#endregion

		#region OnConnectionAccepted
		/// <summary>
		/// Fires the ConnectionAccepted event.
		/// </summary>
		protected void OnConnectionAccepted (Connection connection)
		{
			if (ConnectionAccepted != null)
			{
				ConnectionAcceptedEventArgs e = new ConnectionAcceptedEventArgs (connection);
				ConnectionAccepted (
					this,
					e);
			}
		}
		#endregion

		#region PackageReceived
		/// <summary>
		/// This event is fired when any connection of the networkadapter received tempDataBuffer.
		/// </summary>
		public event PackageReceivedEventHandler PackageReceived; 
		#endregion

		#region ConnectionAccepted
		/// <summary>
		/// The event is fired when a new connection is accepted by a listening socket.
		/// </summary>
		public event ConnectionAcceptedEventHandler ConnectionAccepted;
		#endregion

		#region SendPackage
		/// <summary>
		/// Send a package to the far end point of the passed connection.
		/// </summary>
		public Package SendPackage (
			Package package, 
			Connection connection, 
			Boolean waitForReply)
		{
			return connection.Send (package, waitForReply);
		}
		#endregion

		#region OnAfterConnectionClosed
		/// <summary>
		/// Fires the AfterConnectionClose event.
		/// </summary>
		protected void OnAfterConnectionClosed (Connection connection)
		{
			if (AfterConnectionClosed != null)
			{
				AfterConnectionClosedEventArgs e = new AfterConnectionClosedEventArgs (
					connection);

				AfterConnectionClosed (
					this,
					e);
			}
		}
		#endregion

		#region AfterConnectionClosed
		/// <summary>
		/// Occurs after a connection to the adapter has been closed.
		/// </summary>
		public event AfterConnectionClosedEventHandler AfterConnectionClosed;
		#endregion
	}
}
