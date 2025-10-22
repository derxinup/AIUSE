using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace PlantingWallDemo
{
    public partial class MainForm : Form
    {
        private PlantingSlot[,] _slots = null!;
        private List<OrderData> _orders;
        private TextBox _qrCodeTextBox = null!;
        private Button _importButton = null!;
        private Button _confirmButton = null!;
        private TableLayoutPanel _gridPanel = null!;
        private SpeechSynthesizer _speechSynthesizer;
        
        private const int ROWS = 3;
        private const int COLUMNS = 4;

        public MainForm()
        {
            InitializeComponent();
            InitializePlantingWall();
            _orders = new List<OrderData>();
            _speechSynthesizer = new SpeechSynthesizer();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Form settings
            Text = "人工播种墙演示程序";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            
            // Import button
            _importButton = new Button
            {
                Text = "导入CSV订单数据",
                Location = new Point(20, 20),
                Size = new Size(150, 30),
                BackColor = Color.LightBlue
            };
            _importButton.Click += ImportButton_Click;
            Controls.Add(_importButton);
            
            // QR Code textbox
            var qrLabel = new Label
            {
                Text = "扫码枪输入:",
                Location = new Point(200, 25),
                Size = new Size(80, 20)
            };
            Controls.Add(qrLabel);
            
            _qrCodeTextBox = new TextBox
            {
                Location = new Point(280, 22),
                Size = new Size(200, 25),
                Font = new Font("Microsoft Sans Serif", 10F)
            };
            _qrCodeTextBox.KeyPress += QrCodeTextBox_KeyPress;
            Controls.Add(_qrCodeTextBox);
            
            // Confirm button
            _confirmButton = new Button
            {
                Text = "确认",
                Location = new Point(500, 20),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen
            };
            _confirmButton.Click += ConfirmButton_Click;
            Controls.Add(_confirmButton);
            
            // Grid panel for planting wall
            _gridPanel = new TableLayoutPanel
            {
                Location = new Point(20, 70),
                Size = new Size(720, 360),
                RowCount = ROWS,
                ColumnCount = COLUMNS,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Set equal column and row sizes
            for (int i = 0; i < COLUMNS; i++)
            {
                _gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / COLUMNS));
            }
            for (int i = 0; i < ROWS; i++)
            {
                _gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / ROWS));
            }
            
            Controls.Add(_gridPanel);
            
            // Status info
            var statusLabel = new Label
            {
                Text = "状态: 请导入CSV数据文件开始使用",
                Location = new Point(20, 450),
                Size = new Size(400, 20),
                ForeColor = Color.Blue
            };
            Controls.Add(statusLabel);
            
            // Generate test data button
            var generateTestButton = new Button
            {
                Text = "生成测试CSV文件",
                Location = new Point(600, 20),
                Size = new Size(120, 30),
                BackColor = Color.Yellow
            };
            generateTestButton.Click += GenerateTestButton_Click;
            Controls.Add(generateTestButton);
            
            ResumeLayout();
        }

        private void InitializePlantingWall()
        {
            _slots = new PlantingSlot[ROWS, COLUMNS];
            
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLUMNS; col++)
                {
                    var slot = new PlantingSlot(row, col);
                    _slots[row, col] = slot;
                    _gridPanel.Controls.Add(slot, col, row);
                }
            }
        }

        private void ImportButton_Click(object? sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "选择CSV订单数据文件";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _orders = CsvHelper.ReadOrdersFromCsv(openFileDialog.FileName);
                        LoadOrdersToGrid();
                        MessageBox.Show($"成功导入 {_orders.Count} 条订单数据!", "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadOrdersToGrid()
        {
            // Clear all slots first
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLUMNS; col++)
                {
                    _slots[row, col].ClearAllOrders();
                }
            }
            
            // Distribute orders across all slots evenly
            int totalSlots = ROWS * COLUMNS;
            int orderIndex = 0;
            
            // First, add one order to each slot if available
            for (int row = 0; row < ROWS && orderIndex < _orders.Count; row++)
            {
                for (int col = 0; col < COLUMNS && orderIndex < _orders.Count; col++)
                {
                    _slots[row, col].AddOrder(_orders[orderIndex]);
                    orderIndex++;
                }
            }
            
            // Then distribute remaining orders cyclically
            int slotIndex = 0;
            while (orderIndex < _orders.Count)
            {
                int row = slotIndex / COLUMNS;
                int col = slotIndex % COLUMNS;
                _slots[row, col].AddOrder(_orders[orderIndex]);
                orderIndex++;
                slotIndex = (slotIndex + 1) % totalSlots;
            }
        }

        private void QrCodeTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ProcessQRCode();
                e.Handled = true;
            }
        }

        private void ConfirmButton_Click(object? sender, EventArgs e)
        {
            ProcessQRCode();
        }

        private void ProcessQRCode()
        {
            string qrCode = _qrCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(qrCode))
            {
                MessageBox.Show("请输入或扫描二维码!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Find matching slot
            PlantingSlot? matchedSlot = null;
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLUMNS; col++)
                {
                    if (_slots[row, col].MatchesQRCode(qrCode))
                    {
                        matchedSlot = _slots[row, col];
                        break;
                    }
                }
                if (matchedSlot != null) break;
            }

            if (matchedSlot != null)
            {
                // Highlight the slot
                matchedSlot.Highlight();
                
                // Play speech announcement
                string announcement = $"{matchedSlot.Row + 1}行{matchedSlot.Column + 1}列";
                PlayAnnouncement(announcement);
                
                // Remove the specific order after a short delay
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, e) =>
                {
                    matchedSlot.RemoveOrder(qrCode);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
                
                // Clear the textbox
                _qrCodeTextBox.Clear();
                _qrCodeTextBox.Focus();
                
                MessageBox.Show($"订单确认成功! 位置: {announcement}", "确认成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("未找到匹配的订单!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlayAnnouncement(string text)
        {
            try
            {
                _speechSynthesizer.SpeakAsync(text);
            }
            catch (Exception)
            {
                // If speech synthesis fails, use system beep as fallback
                SystemSounds.Beep.Play();
            }
        }

        private void GenerateTestButton_Click(object? sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.Title = "保存测试CSV文件";
                saveFileDialog.FileName = "test_orders.csv";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        CsvHelper.GenerateTestCsv(saveFileDialog.FileName);
                        MessageBox.Show($"测试CSV文件已生成: {saveFileDialog.FileName}", "生成成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"生成失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _speechSynthesizer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}