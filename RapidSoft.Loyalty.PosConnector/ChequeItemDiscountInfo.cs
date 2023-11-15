using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeItemDiscountInfo", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ChequeItemDiscountInfo : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string ArticleIdField;

		private AdditionalField[] CustomFieldsField;

		private decimal DiscountField;

		[DataMember]
		public string ArticleId
		{
			get
			{
				return this.ArticleIdField;
			}
			set
			{
				this.ArticleIdField = value;
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

		[DataMember]
		public decimal Discount
		{
			get
			{
				return this.DiscountField;
			}
			set
			{
				this.DiscountField = value;
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

		public ChequeItemDiscountInfo()
		{
		}
	}
}