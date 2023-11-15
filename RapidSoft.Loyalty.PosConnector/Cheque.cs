using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="Cheque", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class Cheque : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private DateTime ChequeDateTimeField;

		private decimal ChequeDiscountField;

		private string ChequeNumberField;

		private decimal ChequeSumField;

		private string CurrencyField;

		private AdditionalField[] CustomFieldsField;

		private ChequeItem[] ItemsField;

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

		[DataMember]
		public AdditionalField[] CustomFields
		{
			get
			{
				return this.CustomFieldsField;
			}
			set
			{
				this.CustomFieldsField = value;
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
		public ChequeItem[] Items
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

		public Cheque()
		{
		}
	}
}