using System;
using System.Collections.Generic;
using System.Text;

namespace ESolutions.Net
{
    /// <summary>
    /// Delegate for the AfterConnectionClosed event handlers.
    /// </summary>
    /// <param name="sender">Object sending the AfterConnectionClosed event.</param>
    /// <param name="e">Paramters for the AfterConnectionClosed event.</param>
	public delegate void AfterConnectionClosedEventHandler (
		object sender,
		AfterConnectionClosedEventArgs e);
}
