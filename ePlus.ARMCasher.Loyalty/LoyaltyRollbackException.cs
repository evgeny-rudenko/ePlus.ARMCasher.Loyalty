using System;

namespace ePlus.ARMCasher.Loyalty
{
	public class LoyaltyRollbackException : Exception
	{
		public LoyaltyRollbackException(string message) : base(message)
		{
		}
	}
}