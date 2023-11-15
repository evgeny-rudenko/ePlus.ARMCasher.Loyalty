using ePlus.ARMCasher.Loyalty;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.GoldenMiddle
{
	internal class GMTransactionData : LpTransactionData
	{
		public string TransactionID
		{
			get;
			private set;
		}

		public GMTransactionData(Guid chequeID, Guid chequeOperationType, string transactionID) : base(chequeID, chequeOperationType)
		{
			this.TransactionID = transactionID;
		}
	}
}