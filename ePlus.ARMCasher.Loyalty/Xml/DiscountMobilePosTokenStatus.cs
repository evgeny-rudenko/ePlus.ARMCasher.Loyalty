using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	[XmlType("root", Namespace="")]
	public class DiscountMobilePosTokenStatus
	{
		[XmlElement("active", Namespace="")]
		public string IsActive;

		public DiscountMobilePosTokenStatus()
		{
		}
	}
}