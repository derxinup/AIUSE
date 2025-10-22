using System;
using System.Drawing;
using System.Windows.Forms;

namespace PlantingWallDemo
{
    /// <summary>
    /// Custom control representing a single slot in the planting wall
    /// </summary>
    public class PlantingSlot : Panel
    {
        private Label _contentLabel = null!;
        private OrderData? _orderData;
        private int _row;
        private int _column;
        
        public int Row => _row;
        public int Column => _column;
        public OrderData? OrderData => _orderData;

        public PlantingSlot(int row, int column)
        {
            _row = row;
            _column = column;
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

        public void SetOrderData(OrderData orderData)
        {
            _orderData = orderData;
            _contentLabel.Text = orderData.ToString();
            BackColor = Color.LightBlue;
        }

        public void ClearOrderData()
        {
            _orderData = null;
            _contentLabel.Text = $"格口 {_row + 1}-{_column + 1}";
            BackColor = Color.LightGray;
        }

        public void Highlight()
        {
            BackColor = Color.LightGreen;
        }

        public bool MatchesQRCode(string qrCode)
        {
            return _orderData != null && _orderData.QRCode.Equals(qrCode, StringComparison.OrdinalIgnoreCase);
        }
    }
}