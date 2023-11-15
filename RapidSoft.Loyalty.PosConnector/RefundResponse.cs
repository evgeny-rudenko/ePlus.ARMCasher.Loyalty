using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RefundResponse", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class RefundResponse : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string ChequeMessageField;

		private string ClientMessageField;

		private System.DateTime DateTimeField;

		private string ErrorField;

		private string OperatorMessageField;

		private int StatusField;

		private System.DateTime UtcDateTimeField;

		[DataMember]
		public string ChequeMessage
		{
			get
			{
				return this.ChequeMessageField;
			}
			set
			{
				this.ChequeMessageField = value;
			}
		}

		[DataMember]
		public string ClientMessage
		{
			get
			{
				return this.ClientMessageField;
			}
			set
			{
				this.ClientMessageField = value;
			}
		}

		[DataMember]
		public System.DateTime DateTime
		{
			get
			{
				return this.DateTimeField;
			}
			set
			{
				this.DateTimeField = value;
			}
		}

		[DataMember]
		public string Error
		{
			get
			{
				return this.ErrorField;
			}
			set
			{
				this.ErrorField = value;
			}
		}

		public ExtensionDataObject ExtensionData
		{
			get
			{
				return this.extensionDataField;
			}
			set
			{
				this.extensionDataField = value;
			}
		}

		[DataMember]
		public string OperatorMessage
		{
			get
			{
				return this.OperatorMessageField;
			}
			set
			{
				this.OperatorMessageField = value;
			}
		}

		[DataMember]
		public int Status
		{
			get
			{
				return this.StatusField;
			}
			set
			{
				this.StatusField = value;
			}
		}

		[DataMember]
		public System.DateTime UtcDateTime
		{
			get
			{
				return this.UtcDateTimeField;
			}
			set
			{
				this.UtcDateTimeField = value;
			}
		}

		public RefundResponse()
		{
		}
	}
}