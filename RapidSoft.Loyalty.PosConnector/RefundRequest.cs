using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RefundRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class RefundRequest : RequestBase
	{
		private string CardDataField;

		private ChequeRefundInfo ChequeField;

		private string OriginalRequestIdField;

		private decimal RefundSumField;

		[DataMember]
		public string CardData
		{
			get
			{
				return this.CardDataField;
			}
			set
			{
				this.CardDataField = value;
			}
		}

		[DataMember]
		public ChequeRefundInfo Cheque
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
		public string OriginalRequestId
		{
			get
			{
				return this.OriginalRequestIdField;
			}
			set
			{
				this.OriginalRequestIdField = value;
			}
		}

		[DataMember]
		public decimal RefundSum
		{
			get
			{
				return this.RefundSumField;
			}
			set
			{
				this.RefundSumField = value;
			}
		}

		public RefundRequest()
		{
		}
	}
}