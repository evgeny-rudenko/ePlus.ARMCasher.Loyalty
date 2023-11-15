using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="RollbackRequest", Namespace="RapidSoft.Loyalty.PosConnector")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public class RollbackRequest : RequestBase
	{
		private string OriginalRequestIdField;

		[DataMember]
		public string OriginalRequestId
		{
			get
			{
				return this.OriginalRequestIdField;
			}
			set
			{
				this.OriginalRequestIdField = value;
			}
		}

		public RollbackRequest()
		{
		}
	}
}