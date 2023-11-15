using ePlus.ARMCasher.BusinessObjects;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class LoyaltyTransaction
	{
		public LpTransactionData Data
		{
			get;
			set;
		}

		public Guid Id
		{
			get;
			private set;
		}

		public OperTypeEnum Operation
		{
			get;
			private set;
		}

		public decimal OperationSum
		{
			get;
			set;
		}

		public LoyaltyTransaction(OperTypeEnum operation)
		{
			this.Id = Guid.NewGuid();
			this.Operation = operation;
		}
	}
}