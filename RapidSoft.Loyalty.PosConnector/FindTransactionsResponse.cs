using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="FindTransactionsResponse", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class FindTransactionsResponse : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string ErrorField;

		private int StatusField;

		private Transaction[] TransactionsField;

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
		public Transaction[] Transactions
		{
			get
			{
				return this.TransactionsField;
			}
			set
			{
				this.TransactionsField = value;
			}
		}

		public FindTransactionsResponse()
		{
		}
	}
}