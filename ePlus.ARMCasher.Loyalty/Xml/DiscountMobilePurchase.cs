using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobilePurchase
	{
		[XmlElement("coupons_url", Namespace="")]
		public string CouponsUrl;

		[XmlElement("date", Namespace="")]
		public string Date;

		[XmlElement("discount", Namespace="")]
		public decimal Discount;

		[XmlElement("doc_id", Namespace="")]
		public string DocId;

		[XmlElement("id", Namespace="")]
		public int Id;

		[XmlElement("pos", Namespace="")]
		public string Pos;

		[XmlElement("sum_bonus", Namespace="")]
		public decimal SumBonus;

		[XmlElement("sum_discount", Namespace="")]
		public decimal SumDiscount;

		[XmlElement("sum_total", Namespace="")]
		public decimal SumTotal;

		[XmlElement("url", Namespace="")]
		public string Url;

		[XmlElement("items", Namespace="")]
		public DiscountMobilePurchaseItemList Items;

		[XmlElement("coupons", Namespace="")]
		public string Coupons;

		public DiscountMobilePurchase()
		{
		}
	}
}