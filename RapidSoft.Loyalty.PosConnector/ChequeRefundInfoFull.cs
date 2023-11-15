using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeRefundInfoFull", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ChequeRefundInfoFull : ChequeRefundInfo
	{
		private DateTime ChequeDateTimeField;

		private string ChequeNumberField;

		private decimal ChequeSumField;

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
		public string ChequeNumber
		{
			get
			{
				return this.ChequeNumberField;
			}
			set
			{
				this.ChequeNumberField = value;
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

		public ChequeRefundInfoFull()
		{
		}
	}
}