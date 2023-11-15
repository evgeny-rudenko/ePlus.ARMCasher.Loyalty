using ePlus.ARMCasher.Loyalty.PCX;
using ePlus.Loyalty;
using System;

namespace ePlus.ARMCasher.Loyalty
{
	public class PcxLpTransResult : LpTransResultBase
	{
		public PcxLpTransResult()
		{
			base.IsRegistered = true;
		}

		public PcxLpTransResult(Guid idChequeGlobal, string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund) : this()
		{
			base.IdChequeGlobal = idChequeGlobal;
			base.ID = id;
			base.ChargedSum = chargedSum;
			base.DebitSum = debitSum;
			base.Balance = balance;
			base.PointsTitle = pointsTitle;
			base.IsRefund = isRefund;
		}

		public PcxLpTransResult(Guid idChequeGlobal, string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle) : this(idChequeGlobal, id, chargedSum, debitSum, balance, pointsTitle, false)
		{
		}

		protected override ILpTransResult Create(string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund)
		{
			PcxLpTransResult pcxLpTransResult = new PcxLpTransResult()
			{
				IdChequeGlobal = base.IdChequeGlobal,
				ID = id,
				ChargedSum = chargedSum,
				DebitSum = debitSum,
				Balance = balance,
				PointsTitle = pointsTitle,
				IsRefund = isRefund
			};
			return pcxLpTransResult;
		}

		protected override string Format(decimal value)
		{
			return PCXUtils.TruncateNonZero(value).ToString();
		}
	}
}