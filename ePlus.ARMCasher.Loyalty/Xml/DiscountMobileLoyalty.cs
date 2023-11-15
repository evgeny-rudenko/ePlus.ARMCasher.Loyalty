using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty.Xml
{
	[Serializable]
	[XmlType("root", Namespace="")]
	public class DiscountMobileLoyalty
	{
		[XmlElement("thresholds")]
		public ThresholdsList Thresholds;

		[XmlElement("amount_to_bonus")]
		public DiscountMobileLoyalty.Amount2BonusList Amount2Bonus;

		[XmlElement("bonus_to_amount")]
		public DiscountMobileLoyalty.Amount2BonusList Bonus2Amount;

		[XmlElement("max_purchase_percentage")]
		public int MaxPurchasePercentage;

		[XmlElement("min_purchase_amount")]
		public int MinPurchaseAmount;

		[XmlElement("type")]
		public string TypeAsString;

		public readonly static List<string> LoyalityProgramTypeAsString;

		public DiscountMobileLoyalty.LoyalityProgramType Type
		{
			get
			{
				int num = DiscountMobileLoyalty.LoyalityProgramTypeAsString.FindIndex((string lt) => lt == this.TypeAsString.ToLower());
				if (num >= 0)
				{
					return (DiscountMobileLoyalty.LoyalityProgramType)num;
				}
				return DiscountMobileLoyalty.LoyalityProgramType.Nothing;
			}
		}

		static DiscountMobileLoyalty()
		{
			List<string> strs = new List<string>()
			{
				"amount",
				"count",
				"bonus",
				"slave",
				"nothing"
			};
			DiscountMobileLoyalty.LoyalityProgramTypeAsString = strs;
		}

		public DiscountMobileLoyalty()
		{
		}

		public class Amount2BonusList
		{
			[XmlElement("list-item")]
			public List<decimal> Items;

			public Amount2BonusList()
			{
			}
		}

		public class Bonus2AmountList
		{
			[XmlElement("list-item")]
			public List<decimal> Items;

			public Bonus2AmountList()
			{
			}
		}

		public class DiscountMobileLoyaltyList
		{
			[XmlElement("list-item")]
			public List<decimal> Items;

			public DiscountMobileLoyaltyList()
			{
			}
		}

		public enum LoyalityProgramType
		{
			Amount,
			Count,
			Bonus,
			Slave,
			Nothing
		}
	}
}