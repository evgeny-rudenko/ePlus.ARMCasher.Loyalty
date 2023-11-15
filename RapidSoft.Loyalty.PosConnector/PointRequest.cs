using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="PointRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	[KnownType(typeof(ApplyDiscountRequest))]
	[KnownType(typeof(FindTransactionsRequest))]
	[KnownType(typeof(GetBalanceRequest))]
	[KnownType(typeof(RefundByChequeRequest))]
	[KnownType(typeof(RefundRequest))]
	[KnownType(typeof(RequestBase))]
	[KnownType(typeof(RollbackRequest))]
	public class PointRequest : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string PartnerIdField;

		private string PosIdField;

		private string TerminalIdField;

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
		public string PartnerId
		{
			get
			{
				return this.PartnerIdField;
			}
			set
			{
				this.PartnerIdField = value;
			}
		}

		[DataMember]
		public string PosId
		{
			get
			{
				return this.PosIdField;
			}
			set
			{
				this.PosIdField = value;
			}
		}

		[DataMember]
		public string TerminalId
		{
			get
			{
				return this.TerminalIdField;
			}
			set
			{
				this.TerminalIdField = value;
			}
		}

		public PointRequest()
		{
		}
	}
}