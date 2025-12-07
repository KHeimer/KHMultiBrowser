using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace MyApp.Controls
{
    public partial class BrowserWithAddressBox : UserControl
    {
        // Zoom-UI-Elemente
        private readonly Button zoomButton;

        private readonly ContextMenuStrip zoomMenu;
        private readonly double[] zoomLevels = new[] { 0.5, 0.75, 1.0, 1.25, 1.5, 2.0 };

        // Placeholder text für modernere Eingabe
        private readonly string placeholderText = "Enter address...";

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
                        textBoxAddress.Text = placeholderText;
                        textBoxAddress.ForeColor = Color.Gray;
                    }

                    textBoxAddress.GotFocus += (s, e) =>
                    {
                        if (textBoxAddress.Text == placeholderText)
                        {
                            textBoxAddress.Text = string.Empty;
                            textBoxAddress.ForeColor = Color.FromArgb(64, 64, 64);
                        }
                    };

                    textBoxAddress.LostFocus += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(textBoxAddress.Text))
                        {
                            textBoxAddress.Text = placeholderText;
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
            }
            catch
            {
                // Styling-Fehler nicht kritisch
            }

            // Füge Button zur Steuerung hinzu
            this.Controls.Add(zoomButton);

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
                    // Stelle Button rechts von textBoxAddress dar. Falls Platzproblem, lege übergeordneten Rechtsabstand fest.
                    var margin = 4;
                    var x = textBoxAddress.Right + margin;
                    var y = textBoxAddress.Top + Math.Max(0, (textBoxAddress.Height - zoomButton.Height) / 2);
                    zoomButton.Location = new Point(x, y);

                    // Wenn Button über den Rand ragt, versuche stattdessen rechts zu verankern
                    if (zoomButton.Right > this.ClientSize.Width)
                    {
                        zoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                        zoomButton.Location = new Point(this.ClientSize.Width - zoomButton.Width - margin, y);
                    }
                    else
                    {
                        zoomButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    }
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
                if (textBoxAddress.Text == placeholderText) return string.Empty;
                return textBoxAddress.Text;
            }
            set
            {
                if (textBoxAddress == null) return;
                if (string.IsNullOrWhiteSpace(value))
                {
                    textBoxAddress.Text = placeholderText;
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
    }
}