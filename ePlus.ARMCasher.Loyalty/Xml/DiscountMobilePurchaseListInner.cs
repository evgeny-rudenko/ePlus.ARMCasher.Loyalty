using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobilePurchaseListInner
	{
		[XmlElement("list-item")]
		public List<DiscountMobilePurchase> Items;

		public DiscountMobilePurchaseListInner()
		{
		}
	}
}