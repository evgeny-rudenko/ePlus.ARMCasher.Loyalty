using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[XmlType("root", Namespace="")]
	public class DiscountMobilePurchaseResponse : DiscountMobilePurchase
	{
		public DiscountMobilePurchaseResponse()
		{
		}
	}
}