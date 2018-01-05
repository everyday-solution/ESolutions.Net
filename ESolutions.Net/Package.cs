using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ESolutions.Net
{
	/// <summary>
	/// Objects of this class represent the tempDataBuffer and communication partners of a package
	/// exchanged between to ip end points.
	/// </summary>
	public class Package
	{
		#region remoteIsWaitingQuery
		/// <summary>
		/// XPath-Query to receive the remoteIsWaiting node.
		/// </summary>
		private const string remoteIsWaitingQuery = "/package/label/remoteIsWaiting";
		#endregion

		#region payloadQuery
		/// <summary>
		/// XPath-Query to receive the payload Xml-Node.
		/// </summary>
		private const string payloadQuery = "/package/payload";
		#endregion 

		#region senderQuery
		/// <summary>
		/// XPath-Query to receive the sender Xml-Node.
		/// </summary>
		private const string senderQuery = "/package/label/sender";
		#endregion

		#region document
		/// <summary>
		/// Contains the tempDataBuffer of the package in a standarized XML-Format
		/// </summary>
		private XmlDocument document;
		#endregion

		#region senderElement
		/// <summary>
		/// Private property used to receive the XmlNode of the base class that represents the sender
		/// of a package. This property can only work correctly if the DOM of the document controlled
		/// by this class and its base class is formatted accordingly to the PackageSchma.xsd
		/// </summary>
		private XmlNode senderElement
		{
			get
			{
				return this.document.SelectSingleNode (senderQuery);
			}
		}
		#endregion

		#region PayloadElement
		/// <summary>
		/// Returns a reference to the XmlNode in which one can find the payload tempDataBuffer of the package.
		/// </summary>
		public XmlNode PayloadElement
		{
			get
			{
				return this.document.SelectSingleNode (payloadQuery);
			}
		} 
		#endregion

		#region remoteIsWaitingNode
		/// <summary>
		/// Gets a reference to the XmlNode in which one can find the information wether the sender of the package is waiting for a reply or not.
		/// </summary>
		private XmlNode remoteIsWaitingNode
		{
			get
			{
				return this.document.SelectSingleNode (remoteIsWaitingQuery);
			}
		}
		#endregion

		#region Sender
		/// <summary>
		/// Ip addrress of sender.
		/// </summary>
		public System.Net.IPAddress Sender
		{
			get
			{
				return System.Net.IPAddress.Parse (senderElement.InnerText);
			}
			set
			{
				senderElement.InnerText = (value as System.Net.IPAddress).ToString ();
			}
		}
		#endregion

		#region RemoteIsWaiting
		/// <summary>
		/// Gets a value indicating if the sender of this package is waiting for a reply.
		/// </summary>
		public Boolean RemoteIsWaiting
		{
			get
			{
				return Convert.ToBoolean (remoteIsWaitingNode.InnerText);
			}
			internal set
			{
				remoteIsWaitingNode.InnerText = value.ToString ();
			}
		}
		#endregion

		#region Payload
		/// <summary>
		/// Gets or sets the payload of the package. This is the tempDataBuffer meant to be transported via
		/// the network.
		/// </summary>
		public String Payload
		{
			get
			{
				return PayloadElement.InnerXml;
			}
			set
			{
				PayloadElement.InnerXml = value;
			}
		}
		#endregion

		#region Package
		/// <summary>
		/// Standard constructor. Prepares the DOM of the controllred document to match the PackageSchema.xsd
		/// </summary>
		public Package ()
		{
			document = new XmlDocument ();

			XmlElement newRootElement = this.document.CreateElement ("package");
			XmlElement newLabelElement = this.document.CreateElement ("label");
			XmlElement newPayloadElement = this.document.CreateElement ("payload");
			XmlElement newSenderElement = this.document.CreateElement ("sender");
			XmlElement newRemoteIsWaiting = this.document.CreateElement ("remoteIsWaiting");

			newRemoteIsWaiting.InnerText = false.ToString ();

			newLabelElement.AppendChild (newSenderElement);
			newLabelElement.AppendChild (newRemoteIsWaiting);

			newRootElement.AppendChild (newLabelElement);
			newRootElement.AppendChild (newPayloadElement);

			this.document.AppendChild (newRootElement);
		} 
		#endregion
		
		#region LoadXml
		/// <summary>
		/// Loads the tempDataBuffer from the memorystream into the internal xml document
		/// </summary>
		/// <param name="data">Data to be loaded</param>
		/// <update author="Tobias Mundt" date="2006-12-06">Added a try catch branch to detect missformed packages.</update>
		internal void LoadXml (byte[] data)
		{
			try
			{
				String message = UTF8Encoding.UTF8.GetString (data);
				this.document.LoadXml (message);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine (ex.Message);
			}
		}
		#endregion

		#region ToTransmissionString
		/// <summary>
		/// Exports the internal xml document into a UTF-8 encoded byte array.
		/// </summary>
		/// <remarks>
		/// The first ten bytes contain a UTF-8 encodes information how long the following
		/// message is in bytes. This structure is used to determine how long a transmission
		/// is. So that multiple transmissions can be separated by the receiver.
		/// </remarks>
		/// <update author="Tobias Mundt" date="2006-12-06">Changed the calculation for the transmission length.</update>
		public byte[] ToTranmissionString ()
		{
			Byte[] message = UTF8Encoding.UTF8.GetBytes (this.document.OuterXml);
			Byte[] length = GetLengthAsUtf8EncodedByteArray (message.Length);
			Byte[] result = new Byte[length.Length + message.Length];

			Array.Copy (
				length,
				0,
				result,
				0,
				length.Length);

			Array.Copy (
				message,
				0,
				result,
				10,
				message.Length);
			
			return result;
		}
		#endregion

		#region GetLengthAsUtf8EncodedByteArray
		/// <summary>
		/// Convertes a Int32 to a UTF8 encoded byte array. The array has a length of ten and is filled from left to right.
		/// All bytes that are not in use are filled with zero. Mention that the length is in clear text.
		/// </summary>
		private Byte[] GetLengthAsUtf8EncodedByteArray (Int32 length)
		{
			//Initialize
			Byte[] result = new Byte[10];

			for (Int32 index = 0; index < 10; index++)
			{
				result[index] = 0;
			}

			//Encode and copy length information to result array
			byte[] encodedLength = UTF8Encoding.UTF8.GetBytes (length.ToString ());

			Array.Copy (
				encodedLength,
				result,
				encodedLength.Length);

			return result;
		}
		#endregion
	}
}
