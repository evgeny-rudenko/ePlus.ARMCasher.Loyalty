using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca
{
	internal class AllowedBarcode
	{
		public string BARCODE
		{
			get;
			set;
		}

		public Guid ID_GOODS_GLOBAL
		{
			get;
			set;
		}

		public AllowedBarcode()
		{
		}
	}
}