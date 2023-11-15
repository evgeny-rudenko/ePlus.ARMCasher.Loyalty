using System;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	public class DiscountMobileCouponItem
	{
		[XmlElement("coupon_condition")]
		public string CouponCondition;

		[XmlElement("date_bought")]
		public string DateBought;

		[XmlElement("date_expiration")]
		public string DateExpiration;

		[XmlElement("date_used")]
		public string DateUsed;

		[XmlElement("id")]
		public int Id;

		[XmlElement("number")]
		public string Number;

		[XmlElement("offer_name")]
		public string OfferName;

		[XmlElement("status")]
		public string Status;

		[XmlElement("url")]
		public string Url;

		public DiscountMobileCouponItem()
		{
		}
	}
}