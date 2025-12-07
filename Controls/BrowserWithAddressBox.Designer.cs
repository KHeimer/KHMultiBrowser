using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace MyApp.Controls
{
    partial class BrowserWithAddressBox
    {
        private IContainer components = null;
        private TextBox textBoxAddress;
        private WebView2 webView;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.textBoxAddress = new TextBox();
            this.webView = new WebView2();

            // textBoxAddress
            this.textBoxAddress.Dock = DockStyle.Top;
            this.textBoxAddress.Height = 26;
            this.textBoxAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.TabIndex = 0;
            this.textBoxAddress.KeyDown += new KeyEventHandler(this.textBoxAddress_KeyDown);

            // webView
            this.webView.Dock = DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 26);
            this.webView.Name = "webView";
            this.webView.TabIndex = 1;

            // BrowserWithAddressBox
            this.Controls.Add(this.webView);
            this.Controls.Add(this.textBoxAddress);
            this.Name = "BrowserWithAddressBox";
            this.Size = new System.Drawing.Size(600, 400);
        }
    }
}