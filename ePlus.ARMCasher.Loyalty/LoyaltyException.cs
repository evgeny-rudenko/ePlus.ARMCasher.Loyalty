using ePlus.Loyalty;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class LoyaltyException : ApplicationException
	{
		public ILoyaltyProgram LoyaltyProgram
		{
			get;
			private set;
		}

		public override string Message
		{
			get
			{
				return string.Format("Ошибка программы лояльности {0} {1}:{2}", this.LoyaltyProgram.LoyaltyType, this.LoyaltyProgram.Name, base.Message);
			}
		}

		public LoyaltyException(ILoyaltyProgram loyaltyProgram)
		{
			this.LoyaltyProgram = loyaltyProgram;
		}

		public LoyaltyException(ILoyaltyProgram loyaltyProgram, string message) : base(message)
		{
			this.LoyaltyProgram = loyaltyProgram;
		}

		public LoyaltyException(ILoyaltyProgram loyaltyProgram, string message, Exception innerException) : base(message, innerException)
		{
			this.LoyaltyProgram = loyaltyProgram;
		}
	}
}