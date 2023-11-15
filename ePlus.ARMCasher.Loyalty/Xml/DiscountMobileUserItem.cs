using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobileUserItem
	{
		[XmlElement("purchases")]
		public int Purchases;

		[XmlElement("first_name")]
		public string FirstName;

		[XmlElement("last_name")]
		public string LastName;

		[XmlElement("middle_name")]
		public string MiddleName;

		[XmlElement("bonus")]
		public int Bonus;

		[XmlElement("purchases_url")]
		public string PurchasesUrl;

		[XmlElement("discount")]
		public int Discounts;

		[XmlElement("amount")]
		public decimal Amount;

		[XmlElement("url")]
		public string Url;

		[XmlElement("loyalty_url")]
		public string LoyaltyUrl;

		[XmlElement("coupons_url")]
		public string CouponsUrl;

		[XmlElement("id")]
		public int Id;

		[XmlElement("card")]
		public string Card;

		public DiscountMobileUserItem()
		{
		}
	}
}