using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="GetBalanceResponse", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class GetBalanceResponse : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private int CardStatusField;

		private string ClientMessageField;

		private AdditionalField[] CustomFieldsField;

		private System.DateTime DateTimeField;

		private string ErrorField;

		private string ExternalCardNumberField;

		private int ExternalProgramIdField;

		private decimal MinSumField;

		private decimal MoneyAuthLimitField;

		private decimal MoneyBalanceField;

		private string MoneyCurrencyField;

		private decimal PointsAuthLimitField;

		private decimal PointsBalanceField;

		private string PointsCurrencyField;

		private int StatusField;

		private System.DateTime UtcDateTimeField;

		[DataMember]
		public int CardStatus
		{
			get
			{
				return this.CardStatusField;
			}
			set
			{
				this.CardStatusField = value;
			}
		}

		[DataMember]
		public string ClientMessage
		{
			get
			{
				return this.ClientMessageField;
			}
			set
			{
				this.ClientMessageField = value;
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
		public System.DateTime DateTime
		{
			get
			{
				return this.DateTimeField;
			}
			set
			{
				this.DateTimeField = value;
			}
		}

		[DataMember]
		public string Error
		{
			get
			{
				return this.ErrorField;
			}
			set
			{
				this.ErrorField = value;
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
		public string ExternalCardNumber
		{
			get
			{
				return this.ExternalCardNumberField;
			}
			set
			{
				this.ExternalCardNumberField = value;
			}
		}

		[DataMember]
		public int ExternalProgramId
		{
			get
			{
				return this.ExternalProgramIdField;
			}
			set
			{
				this.ExternalProgramIdField = value;
			}
		}

		[DataMember]
		public decimal MinSum
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

		[DataMember]
		public decimal MoneyAuthLimit
		{
			get
			{
				return this.MoneyAuthLimitField;
			}
			set
			{
				this.MoneyAuthLimitField = value;
			}
		}

		[DataMember]
		public decimal MoneyBalance
		{
			get
			{
				return this.MoneyBalanceField;
			}
			set
			{
				this.MoneyBalanceField = value;
			}
		}

		[DataMember]
		public string MoneyCurrency
		{
			get
			{
				return this.MoneyCurrencyField;
			}
			set
			{
				this.MoneyCurrencyField = value;
			}
		}

		[DataMember]
		public decimal PointsAuthLimit
		{
			get
			{
				return this.PointsAuthLimitField;
			}
			set
			{
				this.PointsAuthLimitField = value;
			}
		}

		[DataMember]
		public decimal PointsBalance
		{
			get
			{
				return this.PointsBalanceField;
			}
			set
			{
				this.PointsBalanceField = value;
			}
		}

		[DataMember]
		public string PointsCurrency
		{
			get
			{
				return this.PointsCurrencyField;
			}
			set
			{
				this.PointsCurrencyField = value;
			}
		}

		[DataMember]
		public int Status
		{
			get
			{
				return this.StatusField;
			}
			set
			{
				this.StatusField = value;
			}
		}

		[DataMember]
		public System.DateTime UtcDateTime
		{
			get
			{
				return this.UtcDateTimeField;
			}
			set
			{
				this.UtcDateTimeField = value;
			}
		}

		public GetBalanceResponse()
		{
		}
	}
}