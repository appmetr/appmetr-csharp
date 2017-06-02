﻿namespace AppmetrCS.Actions
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

        [DataMember(Name = "appCurrencyCode")]
        public String AppCurrencyCode { get; set; }

        [DataMember(Name = "appCurrencyAmount")]
        public String AppCurrencyAmount { get; set; }
        
        protected TrackPayment()
        {
        }

        public TrackPayment(String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount)
            : this(orderId, transactionId, processor, psUserSpentCurrencyCode, psUserSpentCurrencyAmount, null, null)
        {
        }

        public TrackPayment(String orderId,
            String transactionId,
            String processor,
            String psUserSpentCurrencyCode,
            String psUserSpentCurrencyAmount,
            String appCurrencyCode,
            String appCurrencyAmount) : base(ACTION)
        {
            OrderId = orderId;
            TransactionId = transactionId;
            Processor = processor;
            PsUserSpentCurrencyCode = psUserSpentCurrencyCode;
            PsUserSpentCurrencyAmount = psUserSpentCurrencyAmount;
            AppCurrencyCode = appCurrencyCode;
            AppCurrencyAmount = appCurrencyAmount;
        }


        public override Int32 CalcApproximateSize()
        {
            return base.CalcApproximateSize()
                   + GetStringLength(OrderId)
                   + GetStringLength(TransactionId)
                   + GetStringLength(Processor)
                   + GetStringLength(PsUserSpentCurrencyCode)
                   + GetStringLength(PsUserSpentCurrencyAmount)
                   + GetStringLength(AppCurrencyCode)
                   + GetStringLength(AppCurrencyAmount);
        }
                
        public override String ToString()
        {
            return $"{base.ToString()},orderId={OrderId}, transactionId={TransactionId}, processor={Processor}, " +
                   $"psUserSpentCurrencyCode={PsUserSpentCurrencyCode}, psUserSpentCurrncyAmout={PsUserSpentCurrencyAmount}, " +
                   $"appCurrencyCode={AppCurrencyCode}, appCurrencyAmount={AppCurrencyAmount}";
        }
    }
}