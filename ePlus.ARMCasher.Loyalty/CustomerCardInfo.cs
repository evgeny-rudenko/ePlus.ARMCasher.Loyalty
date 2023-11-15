using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class CustomerCardInfo
	{
		public string ClientId
		{
			get;
			set;
		}

		public string Last4Digit
		{
			get;
			set;
		}

		public string Promocode
		{
			get;
			set;
		}

		public CustomerCardInfo()
		{
		}
	}
}