using ePlus.Loyalty;
using System;

namespace ePlus.ARMCasher.Loyalty
{
	public class LoyaltyCardIsBlockedException : LoyaltyException
	{
		public LoyaltyCardIsBlockedException(ILoyaltyProgram lp, string message, Exception innerException) : base(lp, message, innerException)
		{
		}

		public LoyaltyCardIsBlockedException(ILoyaltyProgram lp, Exception innerException) : this(lp, "Карта \"Заблокирована\". Использование для списания/начисления невозможно.", innerException)
		{
		}

		public LoyaltyCardIsBlockedException(ILoyaltyProgram lp) : this(lp, null)
		{
		}
	}
}