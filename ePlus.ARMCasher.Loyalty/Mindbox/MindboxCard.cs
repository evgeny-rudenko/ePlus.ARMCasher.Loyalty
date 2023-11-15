using ePlus.ARMBusinessLogic;
using ePlus.Interfaces;
using ePlus.Loyalty;
using ePlus.Loyalty.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	public class MindboxCard : LoyaltyCard, ILoyaltyMessageList, ILoyaltyPromocodeList
	{
		public MindboxLoyaltyProgram loyaltyProgram;

		private HashSet<MindboxRecommendation> recommendations = new HashSet<MindboxRecommendation>();

		private HashSet<ILoyaltyMessage> messages = new HashSet<ILoyaltyMessage>();

		private List<IPromocode> promocodes = new List<IPromocode>();

		public string CustomerExternalId
		{
			get;
			set;
		}

		public override ePlus.Loyalty.LoyaltyType LoyaltyType
		{
			get
			{
				return ePlus.Loyalty.LoyaltyType.Mindbox;
			}
		}

		public IEnumerable<IPromocode> Promocodes
		{
			get
			{
				return this.promocodes;
			}
		}

		public MindboxCard()
		{
		}

		public bool AddPromocode(string promocode)
		{
			if (this.promocodes.FirstOrDefault<IPromocode>((IPromocode c) => c.Id == promocode) != null)
			{
				return false;
			}
			DiscountPromocode discountPromocode = new DiscountPromocode()
			{
				Id = promocode
			};
			this.promocodes.Add(discountPromocode);
			return true;
		}

		public bool ChangePromocodeStatus(string promocode, PromocodeStatus status)
		{
			IPromocode promocode1 = this.promocodes.FirstOrDefault<IPromocode>((IPromocode c) => c.Id == promocode);
			if (promocode1 == null)
			{
				return false;
			}
			promocode1.Status = status;
			return true;
		}

		void ePlus.Loyalty.ILoyaltyMessageList.Add(ILoyaltyMessage message)
		{
			this.messages.Add(message);
		}

		void ePlus.Loyalty.ILoyaltyMessageList.Clear()
		{
			throw new NotImplementedException();
		}

		IEnumerable<ILoyaltyMessage> ePlus.Loyalty.ILoyaltyMessageList.GetMessages()
		{
			return this.messages;
		}

		private IEnumerable<IRecommendation> GetRecommendations()
		{
			List<string> list = (
				from r in this.recommendations
				select r.CodeTo).ToList<string>();
			IEnumerable<MindboxRecommendation> mindboxRecommendations = 
				from r in this.recommendations
				where !list.Contains(r.Code)
				select r;
			return (
				from r in mindboxRecommendations
				orderby r.Marginality descending, r.Price descending, r.GoodsName
				select r).ToList<MindboxRecommendation>();
		}
	}
}