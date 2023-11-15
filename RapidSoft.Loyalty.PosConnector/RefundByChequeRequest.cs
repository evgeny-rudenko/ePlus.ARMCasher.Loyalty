using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RefundByChequeRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class RefundByChequeRequest : RequestBase
	{
		private string CardDataField;

		private ChequeRefundInfoFull ChequeField;

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
		public ChequeRefundInfoFull Cheque
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

		public RefundByChequeRequest()
		{
		}
	}
}