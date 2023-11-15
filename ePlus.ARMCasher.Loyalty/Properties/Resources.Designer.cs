using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.Properties
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static string CustomerProcessingStatus_NotFound
		{
			get
			{
				return Resources.ResourceManager.GetString("CustomerProcessingStatus_NotFound", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_AlreadyBoundToAnotherCustomer
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_AlreadyBoundToAnotherCustomer", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_AlreadyBoundToCurrentCustmer
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_AlreadyBoundToCurrentCustmer", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_Bound
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_Bound", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_DiscountCardIsBlocked
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_DiscountCardIsBlocked", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_NotFound
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_NotFound", Resources.resourceCulture);
			}
		}

		internal static string DiscountCardProcessingStatus_NotProcessed
		{
			get
			{
				return Resources.ResourceManager.GetString("DiscountCardProcessingStatus_NotProcessed", Resources.resourceCulture);
			}
		}

		internal static string MindboxCardStatus_Activated
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxCardStatus_Activated", Resources.resourceCulture);
			}
		}

		internal static string MindboxCardStatus_Blocked
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxCardStatus_Blocked", Resources.resourceCulture);
			}
		}

		internal static string MindboxCardStatus_Inactive
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxCardStatus_Inactive", Resources.resourceCulture);
			}
		}

		internal static string MindboxCardStatus_Issued
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxCardStatus_Issued", Resources.resourceCulture);
			}
		}

		internal static string MindboxCardStatus_NotIssued
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxCardStatus_NotIssued", Resources.resourceCulture);
			}
		}

		internal static string MindboxDiscountName_balance
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxDiscountName_balance", Resources.resourceCulture);
			}
		}

		internal static string MindboxDiscountName_promoAction
		{
			get
			{
				return Resources.ResourceManager.GetString("MindboxDiscountName_promoAction", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					Resources.resourceMan = new System.Resources.ResourceManager("ePlus.ARMCasher.Loyalty.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		internal static Bitmap UserRegForm
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("UserRegForm", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}