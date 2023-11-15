using ePlus.ARMBusinessLogic;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;

namespace ePlus.ARMCasher.Loyalty
{
	public class DiscountMobileCard : LoyaltyCard
	{
		private const int DiscountMobileType = 8;

		private decimal _sumDiscount;

		private decimal _sumScore;

		private DiscountMobileCard.CardStates _state;

		private bool _recived;

		private int _discountPercent;

		private int _couponId;

		private List<long> _coupons;

		private decimal _bonusDiscount;

		public List<DiscountMobileCard.DiscountItem> ChequeItems;

		public decimal BonusDiscount
		{
			get
			{
				return this._bonusDiscount;
			}
			set
			{
				this._bonusDiscount = value;
			}
		}

		public int CouponId
		{
			get
			{
				return this._couponId;
			}
			set
			{
				this._couponId = value;
			}
		}

		public List<long> Coupons
		{
			get
			{
				return this._coupons;
			}
			set
			{
				this._coupons = value;
			}
		}

		public string CouponStatusInfo
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int DiscountPercent
		{
			get
			{
				return this._discountPercent;
			}
			set
			{
				this._discountPercent = value;
			}
		}

		public override ePlus.Loyalty.LoyaltyType LoyaltyType
		{
			get
			{
				return ePlus.Loyalty.LoyaltyType.DiscountMobile;
			}
		}

		public bool Recived
		{
			get
			{
				return this._recived;
			}
			set
			{
				this._recived = value;
			}
		}

		public DiscountMobileCard.CardStates State
		{
			get
			{
				return this._state;
			}
			set
			{
				this._state = value;
			}
		}

		public decimal SumDiscount
		{
			get
			{
				return this._sumDiscount;
			}
			set
			{
				this._sumDiscount = value;
			}
		}

		public decimal SumScore
		{
			get
			{
				return this._sumScore;
			}
			set
			{
				this._sumScore = value;
			}
		}

		public DiscountMobileCard()
		{
			this.Coupons = new List<long>();
			this.ChequeItems = new List<DiscountMobileCard.DiscountItem>();
		}

		private string CardStateString()
		{
			string str;
			switch (this.State)
			{
				case DiscountMobileCard.CardStates.Active:
				{
					str = "АКТИВНА";
					break;
				}
				case DiscountMobileCard.CardStates.Used:
				{
					str = "Использована";
					break;
				}
				default:
				{
					str = "Неопределённый статус";
					break;
				}
			}
			return str;
		}

		public override string ToString()
		{
			string str;
			string str1 = string.Concat("Покупатель: ", base.MEMBER_FULLNAME);
			string[] nUMBER = new string[] { "Карта ", base.NUMBER, " ", this.CardStateString(), " " };
			string str2 = string.Concat(nUMBER);
			str = (this.DiscountPercent <= 0 ? string.Concat("Баланс: ", this.SumScore, " ") : string.Concat("Скидка ", this.DiscountPercent, "% "));
			return string.Concat(str2, str, str1);
		}

		public enum CardStates
		{
			Active,
			Used,
			Unknown
		}

		public class DiscountItem
		{
			private long _id;

			private decimal _quantity;

			private decimal _price;

			private decimal _priceDiscounted;

			public long Id
			{
				get
				{
					return this._id;
				}
				set
				{
					this._id = value;
				}
			}

			public decimal Price
			{
				get
				{
					return this._price;
				}
				set
				{
					this._price = value;
				}
			}

			public decimal PriceDiscounted
			{
				get
				{
					return this._priceDiscounted;
				}
				set
				{
					this._priceDiscounted = value;
				}
			}

			public decimal Quantity
			{
				get
				{
					return this._quantity;
				}
				set
				{
					this._quantity = value;
				}
			}

			public DiscountItem()
			{
			}
		}
	}
}