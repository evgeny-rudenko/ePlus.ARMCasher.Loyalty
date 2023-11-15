using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeDiscountInfo", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ChequeDiscountInfo : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private AdditionalField[] CustomFieldsField;

		private decimal? FinalChequeDiscountField;

		private ChequeItemDiscountInfo[] ItemsField;

		private decimal? MinSumField;

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
		public decimal? FinalChequeDiscount
		{
			get
			{
				return this.FinalChequeDiscountField;
			}
			set
			{
				this.FinalChequeDiscountField = value;
			}
		}

		[DataMember]
		public ChequeItemDiscountInfo[] Items
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

		[DataMember]
		public decimal? MinSum
		{
			get
			{
				return this.MinSumField;
			}
			set
			{
				this.MinSumField = value;
			}
		}

		public ChequeDiscountInfo()
		{
		}
	}
}