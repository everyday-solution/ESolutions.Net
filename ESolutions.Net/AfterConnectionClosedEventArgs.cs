using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
    /// <summary>
    /// Arguments for the AfterConnectionClosed event
    /// </summary>
	public class AfterConnectionClosedEventArgs : EventArgs
	{
		#region connection
		/// <summary>
		/// Backing field of Connection. The closed connection.
		/// </summary>
		private Connection connection;
		#endregion

		#region Connection
		/// <summary>
		/// The closed connection.
		/// </summary>
		public Connection Connection
		{
			get
			{
				return connection;
			}
		}
		#endregion

		#region AfterConnectionClosedEventArgs
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connection">The connection that has been closed.</param>
		internal AfterConnectionClosedEventArgs (Connection connection)
		{
			this.connection = connection;
		}
		#endregion
	}
}
