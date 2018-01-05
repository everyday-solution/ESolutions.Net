using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ESolutions.Net.Tests
{
	[TestClass]
	public class NetworkAdapterTests
	{
		[TestMethod]
		public void CreateAdapter()
		{
			var a = 1;
			var b = 2;

			ESolutions.Net.NetworkAdapter adapter = new NetworkAdapter(System.Net.IPAddress.Parse(""));
			ESolutions.Net.Package p = new Package();
			p.p
		}
	}
}
