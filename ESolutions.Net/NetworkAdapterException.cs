using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
	/// <summary>
	/// Exception that can be thrown by the NetworkAdapter class.
	/// </summary>
	public class NetworkAdapterException : Exception
	{
		#region NetworkAdapterException
		/// <summary>
		/// Constructor for a new NetworkAdapterException
		/// </summary>
		/// <param name="message">Message for the new exception</param>
		public NetworkAdapterException (string message) : base (message)
		{
		}
		#endregion

		#region NetworkAdapterException
		/// <summary>
		/// Constructor for a new NetworkAdapterException which follows a previous exception
		/// </summary>
		/// <param name="message">Message for the new exception, the excpetion which occured previously</param>
		/// <param name="innerException"></param>
		public NetworkAdapterException (string message, Exception innerException) : base (message, innerException)
		{
		}
		#endregion
	}
}
