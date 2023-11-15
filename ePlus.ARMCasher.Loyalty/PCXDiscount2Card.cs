using ePlus.ARMBusinessLogic;
using ePlus.Loyalty;
using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class PCXDiscount2Card : LoyaltyCard
	{
		private bool recived;

		public int CLIENT_ID_TYPE
		{
			get;
			set;
		}

		public int DiscountPercent
		{
			get;
			set;
		}

		public override ePlus.Loyalty.LoyaltyType LoyaltyType
		{
			get
			{
				return (ePlus.Loyalty.LoyaltyType)this.CLIENT_ID_TYPE;
			}
		}

		public bool Recived
		{
			get
			{
				return this.recived;
			}
			set
			{
				this.recived = value;
			}
		}

		public string State
		{
			get;
			set;
		}

		public decimal SumScore
		{
			get;
			set;
		}

		public PCXDiscount2Card(int type)
		{
			this.CLIENT_ID_TYPE = type;
		}

		private string GetNameDiscountCard()
		{
			string empty;
			switch (this.LoyaltyType)
			{
				case ePlus.Loyalty.LoyaltyType.Svyaznoy:
				{
					empty = "Связной Клуб";
					break;
				}
				case 5:
				{
					empty = string.Empty;
					break;
				}
				case ePlus.Loyalty.LoyaltyType.Sberbank:
				{
					empty = "Сбербанк";
					break;
				}
				default:
				{
					goto case 5;
				}
			}
			return empty;
		}
	}
}