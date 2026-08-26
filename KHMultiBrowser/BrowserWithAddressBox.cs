using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using KHMultiBrowser;

namespace MyApp.Controls
{
    public partial class BrowserWithAddressBox : UserControl
    {
        // Zoom-UI-Elemente
        private readonly Button zoomButton;
        private readonly Button refreshButton;
        private readonly Button homeButton;

        private readonly ContextMenuStrip zoomMenu;
        private readonly double[] zoomLevels = new[] { 0.5, 0.75, 1.0, 1.25, 1.5, 2.0 };

        // Placeholder text für modernere Eingabe
        private readonly string placeholderText = "textbox.placeholder";

        public BrowserWithAddressBox()
        {
            InitializeComponent();
            // Optional: Standardadresse
            // Address = "https://www.bing.com";

            // Erzeuge Zoom-Button und Menü programmgesteuert, damit Designer-Dateien unangetastet bleiben.
            zoomButton = new Button
            {
                Text = "100%",
                AutoSize = true,
                Height = textBoxAddress?.Height ?? 24,
                Padding = new Padding(6, 0, 6, 0),
            };
            zoomButton.Click += ZoomButton_Click;

            // Refresh-Button erstellen
            refreshButton = new Button
            {
                Text = string.Empty,
                AutoSize = false,
                Width = 32,
                Height = textBoxAddress?.Height ?? 24,
                Padding = new Padding(0),
                Image = CreateRefreshIcon(16, 16),
                ImageAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            refreshButton.Click += RefreshButton_Click;

            // Home-Button erstellen
            homeButton = new Button
            {
                Text = string.Empty,
                AutoSize = false,
                Width = 32,
                Height = textBoxAddress?.Height ?? 24,
                Padding = new Padding(0),
                Image = CreateHomeIcon(16, 16),
                ImageAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            homeButton.Click += HomeButton_Click;

            zoomMenu = new ContextMenuStrip();
            foreach (var level in zoomLevels)
            {
                var percent = (int)(level * 100);
                var item = new ToolStripMenuItem($"{percent}%")
                {
                    Tag = level
                };
                item.Click += ZoomMenuItem_Click;
                zoomMenu.Items.Add(item);
            }

            // Moderne Optik: TextBox und Button stylen
            try
            {
                if (textBoxAddress != null)
                {
                    textBoxAddress.Font = new Font("Segoe UI", 9F);
                    textBoxAddress.BackColor = Color.FromArgb(250, 250, 252);
                    textBoxAddress.BorderStyle = BorderStyle.None;
                    textBoxAddress.ForeColor = Color.FromArgb(64, 64, 64);

                    // Placeholder initialisieren
                    if (string.IsNullOrWhiteSpace(textBoxAddress.Text))
                    {
                        textBoxAddress.Text = StringResources.Instance.Get(placeholderText);
                        textBoxAddress.ForeColor = Color.Gray;
                    }

                    textBoxAddress.GotFocus += (s, e) =>
                    {
                        if (textBoxAddress.Text == StringResources.Instance.Get(placeholderText))
                        {
                            textBoxAddress.Text = string.Empty;
                            textBoxAddress.ForeColor = Color.FromArgb(64, 64, 64);
                        }
                    };

                    textBoxAddress.LostFocus += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(textBoxAddress.Text))
                        {
                            textBoxAddress.Text = StringResources.Instance.Get(placeholderText);
                            textBoxAddress.ForeColor = Color.Gray;
                        }
                    };

                    // Leichter Innenabstand via Margin, falls nötig
                    textBoxAddress.Margin = new Padding(6, 6, 6, 6);
                }

                // Zoom-Button moderner stylen
                zoomButton.FlatStyle = FlatStyle.Flat;
                zoomButton.FlatAppearance.BorderSize = 0;
                zoomButton.BackColor = Color.FromArgb(240, 240, 243);
                zoomButton.ForeColor = Color.FromArgb(50, 50, 50);
                zoomButton.Cursor = Cursors.Hand;
                zoomButton.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
                zoomButton.Padding = new Padding(8, 3, 8, 3);

                // Refresh-Button moderner stylen
                refreshButton.FlatStyle = FlatStyle.Flat;
                refreshButton.FlatAppearance.BorderSize = 0;
                refreshButton.BackColor = Color.FromArgb(240, 240, 243);
                refreshButton.ForeColor = Color.FromArgb(50, 50, 50);
                refreshButton.Cursor = Cursors.Hand;

                // Home-Button moderner stylen
                homeButton.FlatStyle = FlatStyle.Flat;
                homeButton.FlatAppearance.BorderSize = 0;
                homeButton.BackColor = Color.FromArgb(240, 240, 243);
                homeButton.ForeColor = Color.FromArgb(50, 50, 50);
                homeButton.Cursor = Cursors.Hand;
            }
            catch
            {
                // Styling-Fehler nicht kritisch
            }

            // Füge Buttons zur Steuerung hinzu
            this.Controls.Add(zoomButton);
            this.Controls.Add(refreshButton);
            this.Controls.Add(homeButton);

            // Positionierung beim Layout / Resize anpassen
            this.Layout += BrowserWithAddressBox_Layout;
            this.SizeChanged += BrowserWithAddressBox_Layout;
        }

        private void BrowserWithAddressBox_Layout(object? sender, EventArgs e)
        {
            try
            {
                if (textBoxAddress != null)
                {
                    // Stelle Buttons rechts von textBoxAddress dar
                    var margin = 4;
                    var y = textBoxAddress.Top + Math.Max(0, (textBoxAddress.Height - zoomButton.Height) / 2);

                    // Home-Button (am weitesten rechts)
                    var xHome = this.ClientSize.Width - homeButton.Width - margin;
                    homeButton.Location = new Point(xHome, y);
                    homeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                    // Refresh-Button (links vom Home-Button)
                    var xRefresh = xHome - refreshButton.Width - margin;
                    refreshButton.Location = new Point(xRefresh, y);
                    refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                    // Zoom-Button (links vom Refresh-Button)
                    var xZoom = xRefresh - zoomButton.Width - margin;
                    zoomButton.Location = new Point(xZoom, y);
                    zoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                }
            }
            catch
            {
                // Layout-Fehler ignorieren, nicht kritisch
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Address
        {
            get
            {
                if (textBoxAddress == null) return string.Empty;
                var placeholder = StringResources.Instance.Get(placeholderText);
                if (textBoxAddress.Text == placeholder) return string.Empty;
                return textBoxAddress.Text;
            }
            set
            {
                if (textBoxAddress == null) return;
                if (string.IsNullOrWhiteSpace(value))
                {
                    textBoxAddress.Text = StringResources.Instance.Get(placeholderText);
                    textBoxAddress.ForeColor = Color.Gray;
                }
                else
                {
                    textBoxAddress.Text = value;
                    textBoxAddress.ForeColor = Color.FromArgb(64, 64, 64);
                    Navigate(textBoxAddress.Text);
                }
            }
        }

        private void textBoxAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (textBoxAddress != null && textBoxAddress.Text == placeholderText)
                    return; // Keine Navigation für Placeholder

                Navigate(textBoxAddress.Text);
            }
        }

        public async void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            try
            {
                await EnsureBrowserInitializedAsync();
                webView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                // Minimales Fehlerhandling: Zeige Fehlermeldung im Browser-Bereich
                var html = $"<html><body><h2>Navigation fehlgeschlagen</h2><pre>{System.Net.WebUtility.HtmlEncode(ex.Message)}</pre></body></html>";
                try { webView.NavigateToString(html); } catch { /* ignore */ }
            }
        }

        private async Task EnsureBrowserInitializedAsync()
        {
            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }
        }

        // Event: Button-Klick zeigt das Kontextmenü
        private void ZoomButton_Click(object? sender, EventArgs e)
        {
            try
            {
                zoomMenu.Show(zoomButton, new Point(0, zoomButton.Height));
            }
            catch
            {
                // Ignoriere Anzeige-Fehler
            }
        }

        // Event: Zoom-Level ausgewählt
        private async void ZoomMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is double level)
            {
                await SetZoomAsync(level);
            }
        }

        // Public helper: Setzt den Zoom-Faktor (z. B. 1.0 = 100%)
        public async Task SetZoomAsync(double factor)
        {
            if (factor <= 0) return;

            try
            {
                await EnsureBrowserInitializedAsync();
                if (webView != null)
                {
                    webView.ZoomFactor = factor;
                    zoomButton.Text = $"{(int)(factor * 100)}%";
                }
            }
            catch
            {
                // Fehler beim Setzen des Zooms ignorieren (kein Crash)
            }
        }

        public double GetZoom()
        {
            try
            {
                return webView.ZoomFactor;
            }
            catch
            {
                return 1.0; // Standard-Zoom
            }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            Refresh();
        }

        private void HomeButton_Click(object? sender, EventArgs e)
        {
            Address = "about:blank";
        }

        public void Refresh()
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    webView.Reload();
                }
            }
            catch
            {
                // Fehler beim Reload ignorieren
            }
        }

        // Keyboard-Binding für F5 Refresh
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                Refresh();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Erstellt ein einfaches Refresh-Icon (Material Design inspiriert)
        private Bitmap CreateRefreshIcon(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var color = Color.FromArgb(50, 50, 50);
                var pen = new Pen(color, 2f);
                var centerX = width / 2f;
                var centerY = height / 2f;
                var radius = width / 3f;

                // Zeichne einen Kreis mit Pfeil (Refresh-Symbol)
                // Kreis (3/4)
                var rect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
                var startAngle = 45f;
                var sweepAngle = 270f;
                g.DrawArc(pen, rect, startAngle, sweepAngle);

                // Pfeilspitze (oben rechts)
                var arrowTip = new PointF(centerX + radius - 1, centerY - radius + 2);
                var arrowLeft = new PointF(centerX + radius - 4, centerY - radius + 5);
                var arrowBottom = new PointF(centerX + radius - 2, centerY - radius + 7);

                using (var arrowBrush = new SolidBrush(color))
                {
                    var arrowPoints = new[] { arrowTip, arrowLeft, arrowBottom };
                    g.FillPolygon(arrowBrush, arrowPoints);
                }

                pen.Dispose();
            }

            return bitmap;
        }

        // Erstellt ein einfaches Home-Icon (Material Design inspiriert)
        private Bitmap CreateHomeIcon(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var color = Color.FromArgb(50, 50, 50);
                var centerX = width / 2f;
                var centerY = height / 2f;

                // Dach (Dreieck)
                var roofPoints = new[]
                {
                    new PointF(centerX, centerY - 6),      // Spitze oben
                    new PointF(centerX - 6, centerY - 2),  // Links
                    new PointF(centerX + 6, centerY - 2)   // Rechts
                };
                using (var roofBrush = new SolidBrush(color))
                {
                    g.FillPolygon(roofBrush, roofPoints);
                }

                // Haus-Körper (Rechteck)
                using (var bodyBrush = new SolidBrush(color))
                {
                    g.FillRectangle(bodyBrush, centerX - 5, centerY - 2, 10, 8);
                }

                // Tür (kleines Rechteck in der Mitte)
                using (var doorBrush = new SolidBrush(Color.FromArgb(245, 245, 247)))
                {
                    g.FillRectangle(doorBrush, centerX - 2, centerY + 1, 4, 4);
                }

                // Türknauf (kleiner Punkt)
                using (var knobBrush = new SolidBrush(color))
                {
                    g.FillEllipse(knobBrush, centerX + 1, centerY + 2, 1.5f, 1.5f);
                }
            }

            return bitmap;
        }
    }
}