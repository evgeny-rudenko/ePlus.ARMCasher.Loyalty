using ePlus.ARMCasher.BusinessObjects;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;

namespace ePlus.ARMCasher.Loyalty
{
	public class LpTransResult : LpTransResultBase
	{
		public List<PCX_CHEQUE_ITEM> Details = new List<PCX_CHEQUE_ITEM>();

		public LpTransResult(Guid idChequeGlobal, string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund = false)
		{
			base.IdChequeGlobal = idChequeGlobal;
			base.ID = id;
			base.ChargedSum = chargedSum;
			base.DebitSum = debitSum;
			base.Balance = balance;
			base.PointsTitle = pointsTitle;
			base.IsRefund = isRefund;
			base.RoundingInCheque = false;
		}

		public LpTransResult(Guid idChequeGlobal, string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund, bool isRegistered) : this(idChequeGlobal, id, chargedSum, debitSum, balance, pointsTitle, isRefund)
		{
			base.IsRegistered = isRegistered;
		}

		public void AddDetail(Guid idChequeItemGlobal, decimal summ, OperTypeEnum operType, string balanceName, string idPromoaction)
		{
			PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
			{
				CLIENT_ID = base.ID,
				SUMM = summ,
				OPER_TYPE = operType.ToString().ToUpper(),
				BALANCE_NAME = balanceName,
				CLIENT_ID_TYPE = (int)base.LpType,
				ID_CHEQUE_ITEM_GLOBAL = idChequeItemGlobal,
				ID_PROMOACTION = idPromoaction,
				TRANSACTION_ID = base.TransactionId
			};
			this.Details.Add(pCXCHEQUEITEM);
		}

		public void AddDetail(Guid idChequeItemGlobal, decimal summ, OperTypeEnum operType, string balanceName)
		{
			this.AddDetail(idChequeItemGlobal, summ, operType, balanceName, null);
		}

		protected override ILpTransResult Create(string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund)
		{
			LpTransResult lpTransResult = new LpTransResult(base.IdChequeGlobal, id, chargedSum, debitSum, balance, pointsTitle, isRefund)
			{
				RoundingInCheque = base.RoundingInCheque
			};
			return lpTransResult;
		}
	}
}