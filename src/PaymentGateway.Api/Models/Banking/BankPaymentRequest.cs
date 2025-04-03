﻿using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Banking
{
    public class BankPaymentRequest
    {
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; }
        [JsonPropertyName("expiry_date")] 
        public string ExpiryDate{ get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public int Cvv { get; set; }
    }
}
