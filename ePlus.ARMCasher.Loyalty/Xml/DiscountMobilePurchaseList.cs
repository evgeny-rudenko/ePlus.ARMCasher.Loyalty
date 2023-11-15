using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	[XmlType("root", Namespace="")]
	public class DiscountMobilePurchaseList
	{
		[XmlElement("results")]
		public DiscountMobilePurchaseListInner Results;

		public DiscountMobilePurchaseList()
		{
		}
	}
}