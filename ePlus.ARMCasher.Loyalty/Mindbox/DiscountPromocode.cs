using ePlus.Loyalty.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	internal class DiscountPromocode : IPromocode
	{
		public string Id
		{
			get;
			set;
		}

		public PromocodeStatus Status
		{
			get;
			set;
		}

		public DiscountPromocode()
		{
		}
	}
}