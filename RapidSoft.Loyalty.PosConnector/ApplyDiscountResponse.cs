using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ApplyDiscountResponse", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ApplyDiscountResponse : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private int CardStatusField;

		private ChequeDiscountInfo ChequeField;

		private string ChequeMessageField;

		private string ClientMessageField;

		private System.DateTime DateTimeField;

		private string ErrorField;

		private string ExternalCardNumberField;

		private int ExternalProgramIdField;

		private string OperatorMessageField;

		private int StatusField;

		private System.DateTime UtcDateTimeField;

		[DataMember]
		public int CardStatus
		{
			get
			{
				return this.CardStatusField;
			}
			set
			{
				this.CardStatusField = value;
			}
		}

		[DataMember]
		public ChequeDiscountInfo Cheque
		{
			get
			{
				return this.ChequeField;
			}
			set
			{
				this.ChequeField = value;
			}
		}

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
		public string ExternalCardNumber
		{
			get
			{
				return this.ExternalCardNumberField;
			}
			set
			{
				this.ExternalCardNumberField = value;
			}
		}

		[DataMember]
		public int ExternalProgramId
		{
			get
			{
				return this.ExternalProgramIdField;
			}
			set
			{
				this.ExternalProgramIdField = value;
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

		public ApplyDiscountResponse()
		{
		}
	}
}