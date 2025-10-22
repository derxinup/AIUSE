using System;

namespace PlantingWallDemo
{
    /// <summary>
    /// Represents order data for the planting wall
    /// </summary>
    public class OrderData
    {
        public string OrderId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        
        public override string ToString()
        {
            return $"订单: {OrderId}\n产品: {ProductName}\n数量: {Quantity}\n客户: {CustomerName}";
        }
    }
}