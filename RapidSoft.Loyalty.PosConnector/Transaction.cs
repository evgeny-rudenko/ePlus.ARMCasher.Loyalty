using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="Transaction", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class Transaction : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private DateTime ChequeDateTimeField;

		private decimal ChequeDiscountField;

		private decimal ChequeSumField;

		private string TransactionRequestIdField;

		[DataMember]
		public DateTime ChequeDateTime
		{
			get
			{
				return this.ChequeDateTimeField;
			}
			set
			{
				this.ChequeDateTimeField = value;
			}
		}

		[DataMember]
		public decimal ChequeDiscount
		{
			get
			{
				return this.ChequeDiscountField;
			}
			set
			{
				this.ChequeDiscountField = value;
			}
		}

		[DataMember]
		public decimal ChequeSum
		{
			get
			{
				return this.ChequeSumField;
			}
			set
			{
				this.ChequeSumField = value;
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
		public string TransactionRequestId
		{
			get
			{
				return this.TransactionRequestIdField;
			}
			set
			{
				this.TransactionRequestIdField = value;
			}
		}

		public Transaction()
		{
		}
	}
}