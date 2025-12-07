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
            textBoxAddress = new TextBox();
            webView = new WebView2();
            ((ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            // 
            // textBoxAddress
            // 
            textBoxAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxAddress.Location = new Point(0, 3);
            textBoxAddress.Name = "textBoxAddress";
            textBoxAddress.Size = new Size(466, 23);
            textBoxAddress.TabIndex = 0;
            textBoxAddress.KeyDown += textBoxAddress_KeyDown;
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Location = new Point(0, 32);
            webView.Name = "webView";
            webView.Size = new Size(600, 368);
            webView.TabIndex = 1;
            webView.ZoomFactor = 1D;
            // 
            // BrowserWithAddressBox
            // 
            Controls.Add(webView);
            Controls.Add(textBoxAddress);
            Name = "BrowserWithAddressBox";
            Size = new Size(600, 400);
            ((ISupportInitialize)webView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}