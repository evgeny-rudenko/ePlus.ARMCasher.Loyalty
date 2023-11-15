using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	[XmlType("root", Namespace="")]
	public class DiscountMobileUserList
	{
		[XmlElement("list-item")]
		public List<DiscountMobileUserItem> Items;

		public DiscountMobileUserList()
		{
			this.Items = new List<DiscountMobileUserItem>();
		}
	}
}