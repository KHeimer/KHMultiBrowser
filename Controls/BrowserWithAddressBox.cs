using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyApp.Controls
{
    public partial class BrowserWithAddressBox : UserControl
    {
        public BrowserWithAddressBox()
        {
            InitializeComponent();
            // Optional: Standardadresse
            // Address = "https://www.bing.com";
        }

        public string Address
        {
            get => textBoxAddress.Text;
            set
            {
                textBoxAddress.Text = value ?? string.Empty;
                Navigate(textBoxAddress.Text);
            }
        }

        private void textBoxAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
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
    }
}