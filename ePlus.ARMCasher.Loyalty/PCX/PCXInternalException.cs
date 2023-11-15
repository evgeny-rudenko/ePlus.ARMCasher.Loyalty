using ePlus.ARMCasher.Loyalty;
using ePlus.Loyalty;
using System;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	public class PCXInternalException : LoyaltyException
	{
		public PCXInternalException(ILoyaltyProgram loyaltyProgram) : base(loyaltyProgram, "Внутренняя ошибка ПЦ. Состояние счета не было изменено, откат не требуется")
		{
		}
	}
}