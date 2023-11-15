using ePlus.CommonEx.Controls;
using System;
using System.Windows.Forms;

namespace ePlus.ARMCasherNew.Controls
{
	public class ARMPCXBarcodeTextBox : ePlusBarcodeTextBox
	{
		public ARMPCXBarcodeTextBox()
		{
			this.AutoValidating = false;
		}

		protected override void SetText(string text, string base64, byte[] bytes)
		{
			Form form = base.FindForm();
			if (form != null && form.ContainsFocus)
			{
				this.Text = text;
				ePlusBarcodeTextBox.LastBarcode_Set(this.Text);
				ScannerDataReceivedEventArgs scannerDataReceivedEventArg = new ScannerDataReceivedEventArgs(this.Text, BarcodeBelongType.None, base64, bytes);
				this.OnDataReceived(scannerDataReceivedEventArg);
			}
		}
	}
}