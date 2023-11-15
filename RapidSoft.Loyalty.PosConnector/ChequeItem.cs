using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="ChequeItem", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class ChequeItem : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private decimal AmountField;

		private string ArticleIdField;

		private string ArticleNameField;

		private AdditionalField[] CustomFieldsField;

		private decimal ItemSumField;

		[DataMember]
		public decimal Amount
		{
			get
			{
				return this.AmountField;
			}
			set
			{
				this.AmountField = value;
			}
		}

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
		public string ArticleName
		{
			get
			{
				return this.ArticleNameField;
			}
			set
			{
				this.ArticleNameField = value;
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
		public decimal ItemSum
		{
			get
			{
				return this.ItemSumField;
			}
			set
			{
				this.ItemSumField = value;
			}
		}

		public ChequeItem()
		{
		}
	}
}