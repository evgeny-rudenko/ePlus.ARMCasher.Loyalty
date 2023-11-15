using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RequestBase", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	[KnownType(typeof(ApplyDiscountRequest))]
	[KnownType(typeof(FindTransactionsRequest))]
	[KnownType(typeof(GetBalanceRequest))]
	[KnownType(typeof(RefundByChequeRequest))]
	[KnownType(typeof(RefundRequest))]
	[KnownType(typeof(RollbackRequest))]
	public class RequestBase : PointRequest
	{
		private DateTime RequestDateTimeField;

		private string RequestIdField;

		[DataMember(IsRequired=true)]
		public DateTime RequestDateTime
		{
			get
			{
				return this.RequestDateTimeField;
			}
			set
			{
				this.RequestDateTimeField = value;
			}
		}

		[DataMember]
		public string RequestId
		{
			get
			{
				return this.RequestIdField;
			}
			set
			{
				this.RequestIdField = value;
			}
		}

		public RequestBase()
		{
		}
	}
}