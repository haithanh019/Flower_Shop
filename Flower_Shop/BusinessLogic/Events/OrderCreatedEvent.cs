using System;

namespace BusinessLogic.Events
{
    public class OrderCreatedEvent
    {
        public required string OrderNumber { get; set; }
        public required decimal TotalAmount { get; set; }
        public required string CustomerName { get; set; }
    }
}
