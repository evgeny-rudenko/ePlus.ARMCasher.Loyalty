using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace RapidSoft.Loyalty.PosConnector
{
	[DataContract(Name="AdditionalFieldType", Namespace="RapidSoft.Loyalty.PosConnector")]
	[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
	public enum AdditionalFieldType
	{
		[EnumMember]
		Byte,
		[EnumMember]
		DateTime,
		[EnumMember]
		Double,
		[EnumMember]
		Float,
		[EnumMember]
		Int32,
		[EnumMember]
		Int64,
		[EnumMember]
		String
	}
}