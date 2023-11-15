using NLog;
using System;

namespace ePlus.ARMCasher.Loyalty
{
	public static class LoyaltyLogger
	{
		private static Logger _logger;

		private static Logger Log
		{
			get
			{
				Logger logger = LoyaltyLogger._logger;
				if (logger == null)
				{
					logger = LogManager.GetLogger("loyalty_log");
					LoyaltyLogger._logger = logger;
				}
				return logger;
			}
		}

		public static void Error(string message)
		{
			LoyaltyLogger.Log.Error(message);
		}

		public static void Info(string message)
		{
			LoyaltyLogger.Log.Info(message);
		}
	}
}