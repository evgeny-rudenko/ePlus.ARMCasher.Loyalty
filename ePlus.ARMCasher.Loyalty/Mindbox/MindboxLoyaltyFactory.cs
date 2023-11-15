using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCasher.Loyalty.Forms;
using System;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	internal class MindboxLoyaltyFactory
	{
		private IWin32Window parentWindow;

		public MindboxLoyaltyFactory()
		{
		}

		public LoyaltyCard CreateLoyaltyCard()
		{
			throw new NotImplementedException();
		}

		private CardReader GetCardReader(IWin32Window parent)
		{
			this.parentWindow = parent;
			return new CardReader(this.ReadLoyaltyMindboxCard);
		}

		private bool ReadLoyaltyMindboxCard(out CustomerCardInfo customerInfo)
		{
			bool flag;
			customerInfo = null;
			using (FrmScanBarcodeEx frmScanBarcodeEx = new FrmScanBarcodeEx(true)
			{
				Text = "Поиск клиента в программе лояльности"
			})
			{
				if (frmScanBarcodeEx.ShowDialog(this.parentWindow) != DialogResult.OK)
				{
					return false;
				}
				else
				{
					CustomerCardInfo customerCardInfo = new CustomerCardInfo()
					{
						ClientId = frmScanBarcodeEx.Barcode,
						Last4Digit = null,
						Promocode = frmScanBarcodeEx.Promocode
					};
					customerInfo = customerCardInfo;
					flag = true;
				}
			}
			return flag;
		}
	}
}