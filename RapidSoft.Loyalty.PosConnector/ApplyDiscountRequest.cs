using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ApplyDiscountRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ApplyDiscountRequest : RequestBase
	{
		private string CardDataField;

		private RapidSoft.Loyalty.PosConnector.Cheque ChequeField;

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
		public RapidSoft.Loyalty.PosConnector.Cheque Cheque
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

		public ApplyDiscountRequest()
		{
		}
	}
}