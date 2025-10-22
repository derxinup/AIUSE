using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlantingWallDemo
{
    /// <summary>
    /// Custom control representing a single slot in the planting wall
    /// </summary>
    public class PlantingSlot : Panel
    {
        private Label _contentLabel = null!;
        private List<OrderData> _orders;
        private int _row;
        private int _column;
        
        public int Row => _row;
        public int Column => _column;
        public List<OrderData> Orders => _orders;

        public PlantingSlot(int row, int column)
        {
            _row = row;
            _column = column;
            _orders = new List<OrderData>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Size = new Size(150, 100);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.LightGray;
            
            _contentLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular),
                Text = $"格口 {_row + 1}-{_column + 1}",
                BackColor = Color.Transparent
            };
            
            Controls.Add(_contentLabel);
        }

        public void AddOrder(OrderData orderData)
        {
            _orders.Add(orderData);
            UpdateDisplay();
        }

        public void RemoveOrder(string orderIdOrQrCode)
        {
            _orders.RemoveAll(order => 
                order.OrderId.Equals(orderIdOrQrCode, StringComparison.OrdinalIgnoreCase) ||
                order.QRCode.Equals(orderIdOrQrCode, StringComparison.OrdinalIgnoreCase));
            UpdateDisplay();
        }

        public void ClearAllOrders()
        {
            _orders.Clear();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_orders.Count == 0)
            {
                _contentLabel.Text = $"格口 {_row + 1}-{_column + 1}";
                BackColor = Color.LightGray;
            }
            else if (_orders.Count == 1)
            {
                // Single order: show full details
                _contentLabel.Text = _orders[0].ToString();
                BackColor = Color.LightBlue;
            }
            else
            {
                // Multiple orders: show only order numbers
                var orderNumbers = string.Join("\n", _orders.Select(o => o.OrderId));
                _contentLabel.Text = $"格口 {_row + 1}-{_column + 1}\n{orderNumbers}";
                BackColor = Color.LightBlue;
            }
        }

        [Obsolete("Use AddOrder instead")]
        public void SetOrderData(OrderData orderData)
        {
            ClearAllOrders();
            AddOrder(orderData);
        }

        [Obsolete("Use ClearAllOrders instead")]
        public void ClearOrderData()
        {
            ClearAllOrders();
        }

        public void Highlight()
        {
            BackColor = Color.LightGreen;
        }

        public bool MatchesQRCode(string qrCode)
        {
            return _orders.Any(order => 
                order.QRCode.Equals(qrCode, StringComparison.OrdinalIgnoreCase) ||
                order.OrderId.Equals(qrCode, StringComparison.OrdinalIgnoreCase));
        }
    }
}