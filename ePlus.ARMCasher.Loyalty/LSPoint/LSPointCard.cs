using ePlus.ARMBusinessLogic;
using System;
using System.Collections.Generic;

namespace ePlus.ARMCasher.Loyalty.LSPoint
{
	public class LSPointCard : DISCOUNT2_CARD_POLICY
	{
		private const int LSPointCardType = 10;

		private decimal _sumDiscount;

		private decimal _sumScore;

		private LSPointCard.CardStates _state;

		private bool _recived;

		private int _discountPercent;

		private decimal _bonusDiscount;

		public List<LSPointCard.DiscountItem> ChequeItems;

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

		public int ClientTypeId
		{
			get
			{
				return 10;
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

		public LSPointCard.CardStates State
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

		public LSPointCard()
		{
		}

		private string CardStateString()
		{
			string str;
			switch (this.State)
			{
				case LSPointCard.CardStates.Active:
				{
					str = "АКТИВНА";
					break;
				}
				case LSPointCard.CardStates.Used:
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
			return string.Concat("Карта LSPoint №", base.NUMBER);
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

			private long _quantity;

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

			public long Quantity
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