﻿namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class PolicyNotAvailableException : ApplicationException
    {
        public PolicyNotAvailableException() : base("Policy not available") { }
    }
}
