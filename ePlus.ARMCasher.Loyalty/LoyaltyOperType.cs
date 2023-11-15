using System;

namespace ePlus.ARMCasher.Loyalty
{
	internal struct LoyaltyOperType
	{
		public const string Charge = "CHARGE";

		public const string Debit = "DEBIT";

		public const string RefundDebit = "DEBIT_REFUND";

		public const string CargeRefund = "CHARGE_REFUND";
	}
}