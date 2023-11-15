using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobilePurchaseItem
	{
		[XmlElement("group_code")]
		public string GroupCode
		{
			get;
			set;
		}

		[XmlElement("item_code")]
		public string ItemCode
		{
			get;
			set;
		}

		[XmlElement("item_gtin")]
		public string ItemGtin
		{
			get;
			set;
		}

		[XmlElement("quantity")]
		public string Quantity
		{
			get;
			set;
		}

		[XmlElement("sum_total")]
		public string SumTotal
		{
			get;
			set;
		}

		[XmlElement("sum_with_discount")]
		public string SumWithDiscount
		{
			get;
			set;
		}

		public DiscountMobilePurchaseItem()
		{
		}
	}
}