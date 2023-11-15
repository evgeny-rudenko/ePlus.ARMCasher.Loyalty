using RapidSoft.Loyalty.PosConnector;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

[DebuggerStepThrough]
[GeneratedCode("System.ServiceModel", "4.0.0.0")]
public class PosConnectorClient : ClientBase<PosConnector>, PosConnector
{
	public PosConnectorClient()
	{
	}

	public PosConnectorClient(string endpointConfigurationName) : base(endpointConfigurationName)
	{
	}

	public PosConnectorClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
	{
	}

	public PosConnectorClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
	{
	}

	public PosConnectorClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
	{
	}

	public ApplyDiscountResponse ApplyDiscount(ApplyDiscountRequest request)
	{
		return base.Channel.ApplyDiscount(request);
	}

	public FindTransactionsResponse FindLastTransactions(FindTransactionsRequest request)
	{
		return base.Channel.FindLastTransactions(request);
	}

	public GetBalanceResponse GetBalance(GetBalanceRequest request)
	{
		return base.Channel.GetBalance(request);
	}

	public RefundResponse Refund(RefundRequest request)
	{
		return base.Channel.Refund(request);
	}

	public RefundByChequeResponse RefundByCheque(RefundByChequeRequest request)
	{
		return base.Channel.RefundByCheque(request);
	}

	public RollbackResponse Rollback(RollbackRequest request)
	{
		return base.Channel.Rollback(request);
	}
}