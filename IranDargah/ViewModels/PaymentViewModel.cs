using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IranDargah.ViewModels
{
    public class CreatePayment
    {
        public string merchantID { get; set; }
        public int amount { get; set; }
        public string callbackURL { get; set; }
        public string orderId { get; set; }
        public string cardNumber { get; set; }
        public string mobile { get; set; }
        public string description { get; set; }
    }

    public class CreatePaymentResponse
    {
        public int status { get; set; }
        public string authority { get; set; }
        public string message { get; set; }
    }

    public class PaymentCallBack
    {
        public int code { get; set; }
        public string message { get; set; }
        public string authority { get; set; }
        public int amount { get; set; }
        public string orderId { get; set; }
    }

    public class VerifyPayment
    {
        public string merchantID { get; set; }
        public string authority { get; set; }
        public int amount { get; set; }
        public string orderId { get; set; }
    }

    public class VerifyPaymentResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public string refId { get; set; }
        public string orderId { get; set; }
        public string cardNumber { get; set; }
    }
}
