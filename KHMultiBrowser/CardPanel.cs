using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KHMultiBrowser
{
    // Ein einfacher Panel mit abgerundeten Ecken und leichter Schattenwirkung
    public class CardPanel : Panel
    {
        private int cornerRadius = 8;
        private int shadowDepth = 6;
        private Color shadowColor = Color.FromArgb(50, 0, 0, 0);

        // Hover-Effekte
        private bool hoverEffectEnabled = true;
        private int hoverShadowIncrease = 6;
        private int hoverTranslateY = -4;
        private bool isHovered = false;

        // Animation
        private System.Windows.Forms.Timer animTimer;
        private const int defaultAnimInterval = 15; // ms
        private float currentShadowF;
        private float targetShadowF;
        private float currentTranslateF;
        private float targetTranslateF;
        private Color currentBackColor;
        private Color baseBackColor;
        private Color hoverBackColor = Color.FromArgb(250, 250, 252);
        private float animationLerp = 0.22f; // smoothing factor

        [DefaultValue(8)]
        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [DefaultValue(6)]
        public int ShadowDepth
        {
            get => shadowDepth;
            set
            {
                shadowDepth = Math.Max(0, value);
                currentShadowF = shadowDepth;
                Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "50,0,0,0")]
        public Color ShadowColor
        {
            get => shadowColor;
            set { shadowColor = value; Invalidate(); }
        }

        [DefaultValue(true)]
        public bool HoverEffectEnabled
        {
            get => hoverEffectEnabled;
            set { hoverEffectEnabled = value; }
        }

        [DefaultValue(6)]
        public int HoverShadowIncrease
        {
            get => hoverShadowIncrease;
            set { hoverShadowIncrease = Math.Max(0, value); Invalidate(); }
        }

        [DefaultValue(-4)]
        public int HoverTranslateY
        {
            get => hoverTranslateY;
            set { hoverTranslateY = value; Invalidate(); }
        }

        [DefaultValue(typeof(Color), "250,250,252")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set { hoverBackColor = value; Invalidate(); }
        }

        public CardPanel()
        {
            DoubleBuffered = true;
            baseBackColor = Color.White;
            BackColor = baseBackColor;
            currentBackColor = baseBackColor;
            Padding = new Padding(6);

            // Animation timer
            animTimer = new System.Windows.Forms.Timer { Interval = defaultAnimInterval };
            animTimer.Tick += AnimTimer_Tick;

            // Propagiere Hover-Events von bereits vorhandenen Child-Controls
            this.ControlAdded += CardPanel_ControlAdded;
            this.ControlRemoved += CardPanel_ControlRemoved;

            // Attach existing children (designer scenario)
            foreach (Control c in this.Controls)
                AttachChildMouseEvents(c);

            // Enable mouse events for the panel itself
            this.MouseEnter += CardPanel_MouseEnter;
            this.MouseLeave += CardPanel_MouseLeave;

            // initialize current values
            currentShadowF = shadowDepth;
            targetShadowF = shadowDepth;
            currentTranslateF = 0;
            targetTranslateF = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (animTimer != null)
                {
                    animTimer.Tick -= AnimTimer_Tick;
                    animTimer.Dispose();
                    animTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        private void CardPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            AttachChildMouseEvents(e.Control);
        }

        private void CardPanel_ControlRemoved(object? sender, ControlEventArgs e)
        {
            DetachChildMouseEvents(e.Control);
        }

        private void AttachChildMouseEvents(Control c)
        {
            if (c == null) return;
            c.MouseEnter += Child_MouseEnter;
            c.MouseLeave += Child_MouseLeave;

            // recursively attach for nested children
            foreach (Control child in c.Controls)
                AttachChildMouseEvents(child);
        }

        private void DetachChildMouseEvents(Control c)
        {
            if (c == null) return;
            c.MouseEnter -= Child_MouseEnter;
            c.MouseLeave -= Child_MouseLeave;

            foreach (Control child in c.Controls)
                DetachChildMouseEvents(child);
        }

        private void Child_MouseEnter(object? sender, EventArgs e)
        {
            // treat as entering the panel
            CardPanel_MouseEnter(this, EventArgs.Empty);
        }

        private void Child_MouseLeave(object? sender, EventArgs e)
        {
            // if mouse is no longer over the panel, treat as leave
            var pt = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains(pt))
            {
                CardPanel_MouseLeave(this, EventArgs.Empty);
            }
        }

        private void CardPanel_MouseEnter(object? sender, EventArgs e)
        {
            if (!hoverEffectEnabled) return;
            isHovered = true;
            StartAnimationToHover();
        }

        private void CardPanel_MouseLeave(object? sender, EventArgs e)
        {
            if (!hoverEffectEnabled) return;
            isHovered = false;
            StartAnimationToNormal();
        }

        private void StartAnimationToHover()
        {
            targetShadowF = shadowDepth + hoverShadowIncrease;
            targetTranslateF = hoverTranslateY;
            // target background
            // store base back color if needed
            baseBackColor = BackColor;
            animTimer?.Start();
        }

        private void StartAnimationToNormal()
        {
            targetShadowF = shadowDepth;
            targetTranslateF = 0;
            animTimer?.Start();
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            bool needInvalidate = false;

            // shadow
            var shadowDelta = targetShadowF - currentShadowF;
            if (Math.Abs(shadowDelta) > 0.25f)
            {
                currentShadowF += shadowDelta * animationLerp;
                needInvalidate = true;
            }
            else
            {
                if (currentShadowF != targetShadowF)
                {
                    currentShadowF = targetShadowF;
                    needInvalidate = true;
                }
            }

            // translate
            var transDelta = targetTranslateF - currentTranslateF;
            if (Math.Abs(transDelta) > 0.25f)
            {
                currentTranslateF += transDelta * animationLerp;
                needInvalidate = true;
            }
            else
            {
                if (currentTranslateF != targetTranslateF)
                {
                    currentTranslateF = targetTranslateF;
                    needInvalidate = true;
                }
            }

            // background color interpolate towards hoverBackColor when hovered, else to baseBackColor
            Color targetColor = isHovered ? hoverBackColor : baseBackColor;
            var r = (int)(currentBackColor.R + (targetColor.R - currentBackColor.R) * animationLerp);
            var g = (int)(currentBackColor.G + (targetColor.G - currentBackColor.G) * animationLerp);
            var b = (int)(currentBackColor.B + (targetColor.B - currentBackColor.B) * animationLerp);
            var a = (int)(currentBackColor.A + (targetColor.A - currentBackColor.A) * animationLerp);
            var newColor = Color.FromArgb(a, r, g, b);
            if (newColor != currentBackColor)
            {
                currentBackColor = newColor;
                needInvalidate = true;
            }

            if (needInvalidate)
            {
                Invalidate();
            }
            else
            {
                animTimer?.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // determine current shadow depth and translation (use current animated floats)
            int currentShadow = Math.Max(0, (int)Math.Round(currentShadowF));
            int translateY = (int)Math.Round(currentTranslateF);

            // shadow drawing: draw multiple layers with decreasing alpha to simulate soft shadow
            var layers = Math.Max(3, currentShadow / 2);
            for (int i = layers; i >= 1; i--)
            {
                var alpha = Math.Max(10, shadowColor.A * i / layers);
                var color = Color.FromArgb(alpha, shadowColor.R, shadowColor.G, shadowColor.B);
                using (var brush = new SolidBrush(color))
                {
                    var shadowRect = new Rectangle(i, i + Math.Max(0, -translateY), Width - 1 - i, Height - 1 - i);
                    var pathShadow = GetRoundedRectPath(shadowRect, cornerRadius);
                    g.FillPath(brush, pathShadow);
                }
            }

            // main card rect adjusted for shadow and translation
            var rect = new Rectangle(0, Math.Max(0, -translateY), Width - 1 - currentShadow, Height - 1 - currentShadow);

            using (var brush = new SolidBrush(currentBackColor))
            using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                var path = GetRoundedRectPath(rect, cornerRadius);
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (radius <= 0)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }

            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
