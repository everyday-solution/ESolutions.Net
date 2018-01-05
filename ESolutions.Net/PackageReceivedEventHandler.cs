using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
    /// <summary>
    /// Delegate describing the form of method that can handle the PackageReceived event.
    /// </summary>
    /// <param name="sender">Object sending the event.</param>
    /// <param name="e">The aruguments for the PackageReceived event</param>
	public delegate void PackageReceivedEventHandler (
		object sender,
		PackageReceivedEventArgs e);
}
