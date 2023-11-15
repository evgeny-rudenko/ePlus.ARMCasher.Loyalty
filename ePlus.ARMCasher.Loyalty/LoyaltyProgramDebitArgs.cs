using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.Loyalty;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class LoyaltyProgramDebitArgs
	{
		public CHEQUE Cheque
		{
			get;
			set;
		}

		public string ClientId
		{
			get;
			set;
		}

		public Func<LoyaltyCard, ILoyaltyProgram, bool> DelegateAddLoyaltyCard
		{
			get;
			set;
		}

		public CardReader DelegateCardReader
		{
			get;
			set;
		}

		public Func<decimal> DelegateChequeSum
		{
			get;
			set;
		}

		public Func<ILoyaltyProgram, LoyaltyCard> DelegateCreateLoyaltyCard
		{
			get;
			set;
		}

		public Func<ePlus.Loyalty.LoyaltyType, LoyaltyCard> DelegateGetContainsLoyaltyCard
		{
			get;
			set;
		}

		public Func<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram> DelegateGetContainsLoyaltyProgram
		{
			get;
			set;
		}

		public Func<ILoyaltyProgram, bool> DelegateRemoveLoyaltyProgram
		{
			get;
			set;
		}

		public Guid LoyaltyInstance
		{
			get;
			set;
		}

		public ePlus.Loyalty.LoyaltyType LoyaltyType
		{
			get;
			set;
		}

		public string Promocode
		{
			get;
			set;
		}

		public bool ResetDiscount
		{
			get;
			set;
		}

		public bool UseMaxAllowScoresOnCancel
		{
			get;
			set;
		}

		public LoyaltyProgramDebitArgs()
		{
		}
	}
}