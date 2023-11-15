using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeItemRefundInfo", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ChequeItemRefundInfo : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private decimal AmountField;

		private string ArticleIdField;

		[DataMember]
		public decimal Amount
		{
			get
			{
				return this.AmountField;
			}
			set
			{
				this.AmountField = value;
			}
		}

		[DataMember]
		public string ArticleId
		{
			get
			{
				return this.ArticleIdField;
			}
			set
			{
				this.ArticleIdField = value;
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

		public ChequeItemRefundInfo()
		{
		}
	}
}