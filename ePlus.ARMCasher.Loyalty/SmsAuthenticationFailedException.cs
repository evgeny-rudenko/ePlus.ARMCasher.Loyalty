using ePlus.Loyalty;
using System;

namespace ePlus.ARMCasher.Loyalty
{
	public class SmsAuthenticationFailedException : LoyaltyException
	{
		public SmsAuthenticationFailedException(ILoyaltyProgram where, string what) : base(where, what)
		{
		}
	}
}