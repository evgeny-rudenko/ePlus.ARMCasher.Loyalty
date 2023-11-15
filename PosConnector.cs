using RapidSoft.Loyalty.PosConnector;
using System.CodeDom.Compiler;
using System.ServiceModel;

[GeneratedCode("System.ServiceModel", "4.0.0.0")]
[ServiceContract(Namespace="RapidSoft.Loyalty.PosConnector.Service", ConfigurationName="PosConnector")]
public interface PosConnector
{
	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/ApplyDiscount", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/ApplyDiscountResponse")]
	ApplyDiscountResponse ApplyDiscount(ApplyDiscountRequest request);

	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/FindLastTransactions", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/FindLastTransactionsResponse")]
	FindTransactionsResponse FindLastTransactions(FindTransactionsRequest request);

	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/GetBalance", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/GetBalanceResponse")]
	GetBalanceResponse GetBalance(GetBalanceRequest request);

	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/Refund", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/RefundResponse")]
	RefundResponse Refund(RefundRequest request);

	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/RefundByCheque", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/RefundByChequeResponse")]
	RefundByChequeResponse RefundByCheque(RefundByChequeRequest request);

	[OperationContract(Action="RapidSoft.Loyalty.PosConnector.Service/PosConnector/Rollback", ReplyAction="RapidSoft.Loyalty.PosConnector.Service/PosConnector/RollbackResponse")]
	RollbackResponse Rollback(RollbackRequest request);
}