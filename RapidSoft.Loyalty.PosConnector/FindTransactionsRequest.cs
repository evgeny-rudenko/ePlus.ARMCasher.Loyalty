using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="FindTransactionsRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class FindTransactionsRequest : RequestBase
	{
		private decimal ChequeSumField;

		private decimal MoneySumField;

		private DateTime TransactionDateTimeField;

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

		[DataMember]
		public decimal MoneySum
		{
			get
			{
				return this.MoneySumField;
			}
			set
			{
				this.MoneySumField = value;
			}
		}

		[DataMember]
		public DateTime TransactionDateTime
		{
			get
			{
				return this.TransactionDateTimeField;
			}
			set
			{
				this.TransactionDateTimeField = value;
			}
		}

		public FindTransactionsRequest()
		{
		}
	}
}