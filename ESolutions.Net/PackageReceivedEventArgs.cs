using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
    /// <summary>
    /// Arguments for the PackageReceived event.
    /// </summary>
	public class PackageReceivedEventArgs : EventArgs
	{
		#region Connection
		/// <summary>
		/// Backing field of Connection. The connection that received a package.
		/// </summary>
        private Connection connection;
        #endregion 

		#region Connection
		/// <summary>
		/// The connection that received a package.
		/// </summary>
		public Connection Connection
		{
			get
			{
				return connection;
			}
		} 
		#endregion

		#region Package
        /// <summary>
        /// Backing field of Package. The package that was received.
        /// </summary>
		private Package package;
        #endregion

		#region Package
		/// <summary>
		/// The package that was received.
		/// </summary>
		public Package Package
		{
			get
			{
				return package;
			}
		} 
		#endregion

		#region PackageReceivedEventArgs
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connection">The connection that received the package.</param>
		/// <param name="package">The received package.</param>
		internal PackageReceivedEventArgs (
			Connection connection, 
			Package package)
		{
			this.connection = connection;
			this.package = package;
		} 
		#endregion
	}
}
