using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	public class ThresholdsList
	{
		[XmlElement("list-item")]
		public List<DiscountMobileLoyalty.DiscountMobileLoyaltyList> Listitem
		{
			get;
			set;
		}

		public ThresholdsList()
		{
		}
	}
}