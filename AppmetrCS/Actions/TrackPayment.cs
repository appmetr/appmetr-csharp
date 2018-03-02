namespace AppmetrCS.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class TrackPayment : AppMetrAction
    {
        private const String ACTION = "trackPayment";

        [DataMember(Name = "orderId")]
        public String OrderId { get; set; }

        [DataMember(Name = "transactionId")]
        public String TransactionId { get; set; }

        [DataMember(Name = "processor")]
        public String Processor { get; set; }

        [DataMember(Name = "psUserSpentCurrencyCode")]
        public String PsUserSpentCurrencyCode { get; set; }

        [DataMember(Name = "psUserSpentCurrencyAmount")]
        public String PsUserSpentCurrencyAmount { get; set; }

        [DataMember(Name = "psReceivedCurrencyCode")]
        public String PsReceivedCurrencyCode { get; set; }

        [DataMember(Name = "psReceivedCurrencyAmount")]
        public String PsReceivedCurrencyAmount { get; set; }

        [DataMember(Name = "appCurrencyCode")]
        public String AppCurrencyCode { get; set; }

        [DataMember(Name = "appCurrencyAmount")]
        public String AppCurrencyAmount { get; set; }

        [DataMember(Name = "psUserStoreCountryCode")]
        public String PsUserStoreCountryCode { get; set; }

        [DataMember(Name = "isSandbox")]
        public bool? IsSandbox { get; set; }

        protected TrackPayment() : base(ACTION)
        {
        }

        public TrackPayment(
            String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount,
            String psReceivedCurrencyCode,
            String psReceivedCurrencyAmount,
            String appCurrencyCode,
            String appCurrencyAmount,
            String psUserStoreCountryCode = null,
            bool? isSandbox = null
            ) : this()
        {
            OrderId = orderId;
            TransactionId = transactionId;
            Processor = processor;
            PsUserSpentCurrencyCode = psUserSpentCurrencyCode;
            PsUserSpentCurrencyAmount = psUserSpentCurrencyAmount;
            PsReceivedCurrencyCode = psReceivedCurrencyCode;
            PsReceivedCurrencyAmount = psReceivedCurrencyAmount;
            AppCurrencyCode = appCurrencyCode;
            AppCurrencyAmount = appCurrencyAmount;
            PsUserStoreCountryCode = psUserStoreCountryCode;
            IsSandbox = isSandbox;
        }


        public override Int32 CalcApproximateSize()
        {
            return base.CalcApproximateSize()
                   + GetStringLength(OrderId)
                   + GetStringLength(TransactionId)
                   + GetStringLength(Processor)
                   + GetStringLength(PsUserSpentCurrencyCode)
                   + GetStringLength(PsUserSpentCurrencyAmount)
                   + GetStringLength(PsReceivedCurrencyCode)
                   + GetStringLength(PsReceivedCurrencyAmount)
                   + GetStringLength(AppCurrencyCode)
                   + GetStringLength(AppCurrencyAmount)
                   + GetStringLength(PsUserStoreCountryCode);
        }
                
        public override String ToString()
        {
            return $"{base.ToString()}, " +
                   $"OrderId: {OrderId}, " +
                   $"TransactionId: {TransactionId}, " +
                   $"Processor: {Processor}, " +
                   $"PsUserSpentCurrencyCode: {PsUserSpentCurrencyCode}, " +
                   $"PsUserSpentCurrencyAmount: {PsUserSpentCurrencyAmount}, " +
                   $"PsReceivedCurrencyCode: {PsReceivedCurrencyCode}, " +
                   $"PsReceivedCurrencyAmount: {PsReceivedCurrencyAmount}, " +
                   $"AppCurrencyCode: {AppCurrencyCode}, " +
                   $"AppCurrencyAmount: {AppCurrencyAmount}, " +
                   $"PsUserStoreCountryCode: {PsUserStoreCountryCode}, " +
                   $"IsSandbox: {IsSandbox}";
        }
    }
}
