using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	[XmlType("root", Namespace="")]
	public class DiscountMobileCouponList
	{
		[XmlElement("results")]
		public DiscountMobileCouponList.CouponListResults CouponList;

		[XmlElement("page")]
		public int Page;

		[XmlElement("pages")]
		public int Pages;

		[XmlElement("next")]
		public string NextPage;

		public DiscountMobileCouponList()
		{
			this.CouponList = new DiscountMobileCouponList.CouponListResults()
			{
				Items = new List<DiscountMobileCouponItem>()
			};
		}

		public class CouponListResults
		{
			[XmlElement("list-item")]
			public List<DiscountMobileCouponItem> Items;

			public CouponListResults()
			{
			}
		}
	}
}