using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeRefundInfo", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	[KnownType(typeof(ChequeRefundInfoFull))]
	public class ChequeRefundInfo : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string CurrencyField;

		private ChequeItemRefundInfo[] ItemsField;

		[DataMember]
		public string Currency
		{
			get
			{
				return this.CurrencyField;
			}
			set
			{
				this.CurrencyField = value;
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
		public ChequeItemRefundInfo[] Items
		{
			get
			{
				return this.ItemsField;
			}
			set
			{
				this.ItemsField = value;
			}
		}

		public ChequeRefundInfo()
		{
		}
	}
}