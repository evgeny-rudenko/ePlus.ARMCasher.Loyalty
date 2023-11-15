using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobilePurchaseItemList
	{
		[XmlElement("list-item")]
		public List<DiscountMobilePurchaseItem> Items;

		public DiscountMobilePurchaseItemList()
		{
		}
	}
}