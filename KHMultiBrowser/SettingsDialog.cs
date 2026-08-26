using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KHMultiBrowser
{
    /// <summary>
    /// Dialog to configure grid layout (rows and columns).
    /// </summary>
    public partial class SettingsDialog : Form
    {
        private Label labelRows;
        private Label labelColumns;
        private NumericUpDown numericRows;
        private NumericUpDown numericColumns;
        private Button btnOK;
        private Button btnCancel;

        public int GridRows => (int)numericRows.Value;
        public int GridColumns => (int)numericColumns.Value;

        public SettingsDialog()
        {
            InitializeComponent();
        }

        public void SetValues(int rows, int columns)
        {
            numericRows.Value = rows;
            numericColumns.Value = columns;
        }

        private void InitializeComponent()
        {
            // Form properties
            this.Text = StringResources.Instance.Get("dialog.gridSettings");
            this.Width = 350;
            this.Height = 220;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 247);
            this.Padding = new Padding(16);

            // Rows Label
            labelRows = new Label
            {
                Text = StringResources.Instance.Get("dialog.rows"),
                AutoSize = true,
                Location = new Point(20, 24),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(labelRows);

            // Rows NumericUpDown
            numericRows = new NumericUpDown
            {
                Minimum = AppSettings.MinRows,
                Maximum = AppSettings.MaxRows,
                Value = 3,
                Width = 80,
                Location = new Point(140, 20)
            };
            this.Controls.Add(numericRows);

            // Columns Label
            labelColumns = new Label
            {
                Text = StringResources.Instance.Get("dialog.columns"),
                AutoSize = true,
                Location = new Point(20, 64),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(labelColumns);

            // Columns NumericUpDown
            numericColumns = new NumericUpDown
            {
                Minimum = AppSettings.MinColumns,
                Maximum = AppSettings.MaxColumns,
                Value = 3,
                Width = 80,
                Location = new Point(140, 60)
            };
            this.Controls.Add(numericColumns);

            // OK Button
            btnOK = new Button
            {
                Text = StringResources.Instance.Get("dialog.ok"),
                Width = 90,
                Height = 32,
                Location = new Point(140, 110),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(240, 240, 243),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(btnOK);

            // Cancel Button
            btnCancel = new Button
            {
                Text = StringResources.Instance.Get("dialog.cancel"),
                Width = 90,
                Height = 32,
                Location = new Point(240, 110),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(240, 240, 243),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(btnCancel);

            // Set AcceptButton and CancelButton
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
    }
}