using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RollbackResponse", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class RollbackResponse : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private System.DateTime DateTimeField;

		private int StatusField;

		private System.DateTime UtcDateTimeField;

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

		public RollbackResponse()
		{
		}
	}
}