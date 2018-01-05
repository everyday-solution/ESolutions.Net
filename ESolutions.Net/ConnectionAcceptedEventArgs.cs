using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ESolutions.Net
{
	/// <summary>
	/// Argument for the ConnectionAccepted event of the network adapter class.
	/// </summary>
	public class ConnectionAcceptedEventArgs : EventArgs
	{
		#region connection
		/// <summary>
		/// The newly accepted connection.
		/// </summary>
		private Connection connection;
		#endregion

		#region Connection
		/// <summary>
		/// The newly accepted connection.
		/// </summary>
		public Connection Connection
		{
			get
			{
				return this.connection;
			}
		}
		#endregion

		#region ConnectionAcceptedEventArgs
		/// <summary>
		/// Constructor used to create objects of this class when it's content is already known.
		/// </summary>
		/// <param name="connection">The newly accepted connection.</param>
		internal ConnectionAcceptedEventArgs (Connection connection)
		{
			this.connection = connection;
		}
		#endregion
	}
}
