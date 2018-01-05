using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
	#region ConnectionAcceptedEventHandler
	/// <summary>
	/// Represents the form of methods that are able to handle ConnectionAccepted events.
	/// </summary>
	/// <param name="sender">Object sending the event</param>
	/// <param name="e">Parameters for the event</param>
	public delegate void ConnectionAcceptedEventHandler (
		object sender,
		ConnectionAcceptedEventArgs e);
	#endregion
}
