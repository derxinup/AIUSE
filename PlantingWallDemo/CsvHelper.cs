using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PlantingWallDemo
{
    /// <summary>
    /// Helper class for reading CSV files
    /// </summary>
    public static class CsvHelper
    {
        public static List<OrderData> ReadOrdersFromCsv(string filePath)
        {
            var orders = new List<OrderData>();
            
            if (!File.Exists(filePath))
                return orders;
            
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) // No data or only header
                return orders;
            
            // Skip header line (first line)
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i]);
                if (values.Length >= 6) // Ensure we have enough columns
                {
                    var order = new OrderData
                    {
                        OrderId = values[0],
                        ProductName = values[1],
                        QRCode = values[2],
                        Quantity = int.TryParse(values[3], out int qty) ? qty : 1,
                        CustomerName = values[4],
                        OrderDate = DateTime.TryParse(values[5], out DateTime date) ? date : DateTime.Now
                    };
                    orders.Add(order);
                }
            }
            
            return orders;
        }
        
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            
            result.Add(current.Trim());
            return result.ToArray();
        }

        public static void GenerateTestCsv(string filePath)
        {
            var csvContent = @"订单编号,产品名称,二维码,数量,客户姓名,订单日期
ORD001,苹果种子,QR001,10,张三,2024-01-15
ORD002,橘子种子,QR002,15,李四,2024-01-16
ORD003,梨子种子,QR003,8,王五,2024-01-17
ORD004,葡萄种子,QR004,20,赵六,2024-01-18
ORD005,桃子种子,QR005,12,钱七,2024-01-19
ORD006,香蕉种子,QR006,25,孙八,2024-01-20
ORD007,草莓种子,QR007,30,周九,2024-01-21
ORD008,蓝莓种子,QR008,18,吴十,2024-01-22
ORD009,樱桃种子,QR009,22,郑十一,2024-01-23
ORD010,柠檬种子,QR010,14,陈十二,2024-01-24
ORD011,芒果种子,QR011,16,刘十三,2024-01-25
ORD012,西瓜种子,QR012,28,黄十四,2024-01-26";
            
            File.WriteAllText(filePath, csvContent, System.Text.Encoding.UTF8);
        }
    }
}