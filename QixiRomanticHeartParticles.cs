using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QixiRomanticHeartParticles
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length >= 2 && args[0].Equals("--capture", StringComparison.OrdinalIgnoreCase))
            {
                float captureTime = 7.2f;
                float parsedTime;
                if (args.Length >= 3 && float.TryParse(args[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out parsedTime))
                    captureTime = parsedTime;
                LoveCanvas.SavePreview(args[1], 1600, 900, captureTime);
                return;
            }

            bool preview = args.Length > 0 && args[0].Equals("--preview", StringComparison.OrdinalIgnoreCase);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoveForm(preview));
        }
    }

    internal sealed class LoveForm : Form
    {
        private readonly LoveCanvas canvas;
        private readonly CloseButton closeButton;
        private readonly Timer musicTimer;
        private bool musicStarted;

        public LoveForm(bool preview)
        {
            Text = "七夕浪漫3D爱心粒子";
            BackColor = Color.FromArgb(7, 4, 15);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            if (preview)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                ClientSize = new Size(1440, 810);
                MinimumSize = new Size(1000, 620);
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                Bounds = Screen.PrimaryScreen.Bounds;
            }

            canvas = new LoveCanvas();
            canvas.Dock = DockStyle.Fill;
            Controls.Add(canvas);

            closeButton = new CloseButton();
            closeButton.Size = new Size(48, 48);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Click += delegate { Close(); };
            Controls.Add(closeButton);
            closeButton.BringToFront();

            Resize += delegate { PositionCloseButton(); };
            Shown += delegate
            {
                PositionCloseButton();
                closeButton.BringToFront();
                if (!preview)
                {
                    Bounds = Screen.FromControl(this).Bounds;
                }
            };

            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };

            musicTimer = new Timer();
            musicTimer.Interval = 800;
            musicTimer.Tick += delegate
            {
                musicTimer.Stop();
                StartMusicIfPresent();
            };
            musicTimer.Start();
        }

        private void PositionCloseButton()
        {
            closeButton.Location = new Point(Math.Max(0, ClientSize.Width - closeButton.Width - 22), 20);
        }

        private void StartMusicIfPresent()
        {
            if (musicStarted) return;
            string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "传奇.mp3");
            if (!File.Exists(musicPath)) return;

            string safePath = musicPath.Replace("\"", "\"\"");
            AudioPlayer.Command("open \"" + safePath + "\" type mpegvideo alias love_bgm");
            AudioPlayer.Command("play love_bgm repeat");
            musicStarted = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AudioPlayer.Command("stop love_bgm");
            AudioPlayer.Command("close love_bgm");
            base.OnFormClosing(e);
        }
    }

    internal static class AudioPlayer
    {
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, string buffer, int bufferSize, IntPtr callback);

        public static void Command(string command)
        {
            try { mciSendString(command, null, 0, IntPtr.Zero); }
            catch { }
        }
    }

    internal sealed class CloseButton : Control
    {
        private bool hover;

        public CloseButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "关闭";
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) OnClick(EventArgs.Empty);
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF circle = new RectangleF(4, 4, Width - 8, Height - 8);
            Color fill = hover ? Color.FromArgb(220, 173, 31, 94) : Color.FromArgb(112, 18, 18, 24);
            using (SolidBrush brush = new SolidBrush(fill)) e.Graphics.FillEllipse(brush, circle);
            using (Pen border = new Pen(hover ? Color.FromArgb(255, 255, 120, 179) : Color.FromArgb(145, 218, 213, 224), 1.0f))
                e.Graphics.DrawEllipse(border, circle);
            using (Pen pen = new Pen(Color.White, 2.2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                float c = Width / 2f;
                e.Graphics.DrawLine(pen, c - 6, c - 6, c + 6, c + 6);
                e.Graphics.DrawLine(pen, c + 6, c - 6, c - 6, c + 6);
            }
            if (Focused)
            {
                using (Pen focus = new Pen(Color.FromArgb(220, 220, 171, 255), 1f))
                {
                    focus.DashStyle = DashStyle.Dot;
                    e.Graphics.DrawEllipse(focus, 1, 1, Width - 3, Height - 3);
                }
            }
        }
    }

    internal sealed class Particle
    {
        public float U;
        public float V;
        public float Size;
        public float Phase;
        public float Activation;
        public float Angle;
        public float Depth;
        public float Z;
        public float SourceU;
        public float SourceV;
        public bool Edge;
        public int ColorIndex;
    }

    internal sealed class DustParticle
    {
        public float U;
        public float V;
        public float Size;
        public float Phase;
        public float Lift;
        public int ColorIndex;
    }

    internal sealed class LoveCanvas : Control
    {
        private readonly Timer timer;
        private readonly Random random = new Random(842017);
        private readonly List<Particle> heartParticles = new List<Particle>();
        private readonly List<DustParticle> poolParticles = new List<DustParticle>();
        private readonly List<DustParticle> stars = new List<DustParticle>();
        private readonly DateTime started = DateTime.Now;
        private bool generated;
        private float fixedTime = -1f;

        private static readonly Color[] HeartColors =
        {
            Color.FromArgb(255, 60, 24, 95),
            Color.FromArgb(255, 91, 42, 134),
            Color.FromArgb(255, 128, 53, 151),
            Color.FromArgb(255, 167, 62, 163),
            Color.FromArgb(255, 192, 68, 165),
            Color.FromArgb(255, 240, 107, 168),
            Color.FromArgb(255, 255, 159, 198),
            Color.FromArgb(255, 255, 230, 241)
        };

        private static readonly Color[] PurpleColors =
        {
            Color.FromArgb(255, 73, 39, 157),
            Color.FromArgb(255, 103, 58, 208),
            Color.FromArgb(255, 136, 83, 245),
            Color.FromArgb(255, 174, 122, 255),
            Color.FromArgb(255, 220, 192, 255),
            Color.FromArgb(255, 248, 242, 255)
        };

        public LoveCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(3, 3, 7);
            timer = new Timer();
            timer.Interval = 33;
            timer.Tick += delegate { Invalidate(); };
            timer.Start();
            SizeChanged += delegate { generated = false; };
        }

        public static void SavePreview(string path, int width, int height, float time)
        {
            using (LoveCanvas canvas = new LoveCanvas())
            {
                canvas.timer.Stop();
                canvas.Size = new Size(width, height);
                canvas.fixedTime = time;
                canvas.EnsureParticles();
                using (Bitmap bitmap = new Bitmap(width, height))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    canvas.DrawScene(graphics, time);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    RectangleF closeCircle = new RectangleF(width - 66, 22, 36, 36);
                    using (SolidBrush closeFill = new SolidBrush(Color.FromArgb(112, 18, 18, 24)))
                        graphics.FillEllipse(closeFill, closeCircle);
                    using (Pen closeBorder = new Pen(Color.FromArgb(145, 218, 213, 224), 1.0f))
                        graphics.DrawEllipse(closeBorder, closeCircle);
                    using (Pen closePen = new Pen(Color.White, 2.2f))
                    {
                        closePen.StartCap = LineCap.Round;
                        closePen.EndCap = LineCap.Round;
                        graphics.DrawLine(closePen, width - 54, 34, width - 42, 46);
                        graphics.DrawLine(closePen, width - 42, 34, width - 54, 46);
                    }
                    bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            EnsureParticles();
            float time = fixedTime >= 0 ? fixedTime : (float)(DateTime.Now - started).TotalSeconds;
            DrawScene(e.Graphics, time);
        }

        private void EnsureParticles()
        {
            if (generated || Width < 100 || Height < 100) return;
            generated = true;
            heartParticles.Clear();
            poolParticles.Clear();
            stars.Clear();

            int heartCount = Math.Max(11000, Math.Min(15000, (Width * Height) / 115));
            for (int i = 0; i < heartCount; i++)
            {
                double t = random.NextDouble() * Math.PI * 2.0;
                bool edge = random.NextDouble() < 0.17;
                float u;
                float v;
                if (edge)
                {
                    double radial = 0.91 + random.NextDouble() * 0.12;
                    u = (float)(16.0 * Math.Pow(Math.Sin(t), 3.0) / 17.0 * radial);
                    v = (float)(-(13.0 * Math.Cos(t) - 5.0 * Math.Cos(2.0 * t) -
                                  2.0 * Math.Cos(3.0 * t) - Math.Cos(4.0 * t)) / 17.0 * radial);
                }
                else
                {
                    double hx;
                    double hy;
                    double implicitValue;
                    do
                    {
                        hx = -1.18 + random.NextDouble() * 2.36;
                        hy = -1.08 + random.NextDouble() * 2.27;
                        double sum = hx * hx + hy * hy - 1.0;
                        implicitValue = sum * sum * sum - hx * hx * hy * hy * hy;
                    }
                    while (implicitValue > 0.0);
                    u = (float)(hx * 0.91);
                    v = (float)(-hy * 0.92);
                }
                float particleAngle = edge
                    ? (float)(t + Math.PI * 0.5 + (random.NextDouble() - 0.5) * 1.2)
                    : (float)(random.NextDouble() * Math.PI * 2.0);
                double colorRoll = random.NextDouble();
                int colorIndex = colorRoll < 0.025 ? 0 : colorRoll < 0.075 ? 1 :
                                 colorRoll < 0.25 ? 2 : colorRoll < 0.67 ? 3 :
                                 colorRoll < 0.88 ? 4 : colorRoll < 0.965 ? 5 :
                                 colorRoll < 0.995 ? 6 : 7;
                float normalizedRadius = (float)Math.Sqrt((u * u) / 1.15f + (v * v) / 1.18f);
                float volumeProfile = (float)Math.Pow(
                    Math.Max(0f, 1f - Math.Min(1f, normalizedRadius)), 0.54);
                float thickness = edge ? 0.17f + volumeProfile * 0.12f :
                                  0.16f + volumeProfile * 0.68f;
                float bottomToTop = Clamp((1.08f - v) / 2.14f, 0f, 1f);
                float activation = edge
                    ? 0.018f + bottomToTop * 0.46f + (float)random.NextDouble() * 0.035f
                    : 0.475f + bottomToTop * 0.43f + (float)random.NextDouble() * 0.075f;
                heartParticles.Add(new Particle
                {
                    U = u,
                    V = v,
                    Size = 0.72f + (float)Math.Pow(random.NextDouble(), 2.7) * 2.85f,
                    Phase = (float)random.NextDouble() * 10f,
                    Activation = Clamp(activation, 0f, 1f),
                    Angle = particleAngle,
                    Depth = (float)random.NextDouble(),
                    Z = ((float)random.NextDouble() * 2f - 1f) * thickness,
                    SourceU = (float)(random.NextDouble() * 0.64 - 0.32),
                    SourceV = (float)random.NextDouble(),
                    Edge = edge,
                    ColorIndex = colorIndex
                });
            }

            int poolCount = Math.Max(1800, Math.Min(3000, (Width * Height) / 570));
            for (int i = 0; i < poolCount; i++)
            {
                double theta = random.NextDouble() * Math.PI * 2.0;
                double radius = Math.Sqrt(random.NextDouble());
                poolParticles.Add(new DustParticle
                {
                    U = (float)(Math.Cos(theta) * radius),
                    V = (float)(Math.Sin(theta) * radius),
                    Size = 0.55f + (float)Math.Pow(random.NextDouble(), 2.3) * 3.4f,
                    Phase = (float)random.NextDouble() * 20f,
                    Lift = 16f + (float)random.NextDouble() * 100f,
                    ColorIndex = random.Next(PurpleColors.Length)
                });
            }

            for (int i = 0; i < 108; i++)
            {
                stars.Add(new DustParticle
                {
                    U = (float)random.NextDouble(),
                    V = (float)random.NextDouble(),
                    Size = 0.5f + (float)random.NextDouble() * 1.6f,
                    Phase = (float)random.NextDouble() * 10f,
                    Lift = (float)random.NextDouble(),
                    ColorIndex = random.Next(3)
                });
            }
        }

        private void DrawScene(Graphics g, float time)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height));
            DrawRomanticBackground(g, bounds, time);
            DrawAtmosphere(g, bounds, time);
            DrawStars(g, bounds, time);

            DrawParticleHeart(g, new RectangleF(0, 0, bounds.Width, bounds.Height), time);

            using (LinearGradientBrush vignette = new LinearGradientBrush(
                new Rectangle(0, 0, bounds.Width, Math.Max(1, (int)(bounds.Height * 0.13f))),
                Color.FromArgb(80, 0, 0, 0), Color.Transparent, LinearGradientMode.Vertical))
                g.FillRectangle(vignette, 0, 0, bounds.Width, bounds.Height * 0.13f);

            float sceneFade = SmoothStep(8.75f, 9.92f, time % 10f);
            if (sceneFade > 0.001f)
            {
                using (SolidBrush fadeBrush = new SolidBrush(Color.FromArgb((int)(248 * sceneFade), 3, 3, 7)))
                    g.FillRectangle(fadeBrush, bounds);
            }
        }

        private void DrawRomanticBackground(Graphics g, Rectangle bounds, float time)
        {
            using (LinearGradientBrush night = new LinearGradientBrush(
                bounds, Color.FromArgb(4, 7, 22), Color.FromArgb(24, 10, 47),
                LinearGradientMode.Vertical))
            {
                ColorBlend blend = new ColorBlend();
                blend.Colors = new[]
                {
                    Color.FromArgb(4, 7, 22),
                    Color.FromArgb(8, 9, 31),
                    Color.FromArgb(15, 9, 42),
                    Color.FromArgb(24, 10, 47)
                };
                blend.Positions = new[] { 0f, 0.38f, 0.72f, 1f };
                night.InterpolationColors = blend;
                g.FillRectangle(night, bounds);
            }

            float drift = (float)Math.Sin(time * 0.18f) * bounds.Width * 0.025f;
            using (GraphicsPath upperRibbon = new GraphicsPath())
            {
                upperRibbon.AddBezier(
                    -bounds.Width * 0.12f + drift, bounds.Height * 0.16f,
                    bounds.Width * 0.22f + drift, bounds.Height * 0.48f,
                    bounds.Width * 0.66f + drift, -bounds.Height * 0.06f,
                    bounds.Width * 1.10f + drift, bounds.Height * 0.22f);
                using (Pen haze = new Pen(Color.FromArgb(3, 72, 76, 190), bounds.Height * 0.16f))
                using (Pen aurora = new Pen(Color.FromArgb(5, 116, 80, 214), bounds.Height * 0.065f))
                using (Pen core = new Pen(Color.FromArgb(4, 165, 104, 236), bounds.Height * 0.016f))
                {
                    haze.StartCap = haze.EndCap = LineCap.Round;
                    aurora.StartCap = aurora.EndCap = LineCap.Round;
                    core.StartCap = core.EndCap = LineCap.Round;
                    g.DrawPath(haze, upperRibbon);
                    g.DrawPath(aurora, upperRibbon);
                    g.DrawPath(core, upperRibbon);
                }
            }

            float lowerDrift = (float)Math.Cos(time * 0.14f) * bounds.Width * 0.018f;
            using (GraphicsPath lowerRibbon = new GraphicsPath())
            {
                lowerRibbon.AddBezier(
                    -bounds.Width * 0.10f + lowerDrift, bounds.Height * 0.72f,
                    bounds.Width * 0.24f + lowerDrift, bounds.Height * 0.90f,
                    bounds.Width * 0.72f + lowerDrift, bounds.Height * 0.42f,
                    bounds.Width * 1.08f + lowerDrift, bounds.Height * 0.66f);
                using (Pen haze = new Pen(Color.FromArgb(2, 95, 48, 161), bounds.Height * 0.11f))
                using (Pen aurora = new Pen(Color.FromArgb(4, 158, 64, 174), bounds.Height * 0.035f))
                {
                    haze.StartCap = haze.EndCap = LineCap.Round;
                    aurora.StartCap = aurora.EndCap = LineCap.Round;
                    g.DrawPath(haze, lowerRibbon);
                    g.DrawPath(aurora, lowerRibbon);
                }
            }

            using (LinearGradientBrush leftVignette = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, bounds.Width / 4), bounds.Height),
                Color.FromArgb(92, 1, 2, 10), Color.Transparent, LinearGradientMode.Horizontal))
                g.FillRectangle(leftVignette, 0, 0, bounds.Width / 4f, bounds.Height);
            using (LinearGradientBrush rightVignette = new LinearGradientBrush(
                new Rectangle(Math.Max(0, bounds.Width * 3 / 4), 0, Math.Max(1, bounds.Width / 4), bounds.Height),
                Color.Transparent, Color.FromArgb(92, 1, 2, 10), LinearGradientMode.Horizontal))
                g.FillRectangle(rightVignette, bounds.Width * 0.75f, 0, bounds.Width * 0.25f, bounds.Height);
        }

        private void DrawAtmosphere(Graphics g, Rectangle bounds, float time)
        {
            DrawSoftGlow(g, new PointF(bounds.Width * 0.50f, bounds.Height * 0.82f),
                         bounds.Width * 0.38f, bounds.Height * 0.22f, Color.FromArgb(15, 109, 49, 237));
        }

        private static void DrawSoftGlow(Graphics g, PointF center, float width, float height, Color centerColor)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(center.X - width / 2f, center.Y - height / 2f, width, height);
                using (PathGradientBrush glow = new PathGradientBrush(path))
                {
                    glow.CenterColor = centerColor;
                    glow.SurroundColors = new[] { Color.FromArgb(0, centerColor) };
                    g.FillPath(glow, path);
                }
            }
        }

        private void DrawStars(Graphics g, Rectangle bounds, float time)
        {
            foreach (DustParticle star in stars)
            {
                float alphaPulse = 0.35f + 0.65f * (float)((Math.Sin(time * 1.4f + star.Phase) + 1.0) / 2.0);
                int alpha = (int)(35 + 105 * alphaPulse);
                Color color = star.ColorIndex == 0 ? Color.FromArgb(alpha, 232, 214, 255) :
                              star.ColorIndex == 1 ? Color.FromArgb(alpha, 255, 199, 230) :
                              Color.FromArgb(alpha, 173, 133, 255);
                float x = bounds.Width * star.U;
                float y = bounds.Height * star.V;
                using (SolidBrush brush = new SolidBrush(color))
                    g.FillEllipse(brush, x, y, star.Size, star.Size);
            }
        }

        private static void DrawDivider(Graphics g, float x, float height)
        {
            using (LinearGradientBrush line = new LinearGradientBrush(
                new RectangleF(x, height * 0.12f, 1f, height * 0.76f),
                Color.Transparent, Color.FromArgb(115, 206, 173, 255), LinearGradientMode.Vertical))
            {
                ColorBlend blend = new ColorBlend();
                blend.Colors = new[] { Color.Transparent, Color.FromArgb(95, 206, 173, 255), Color.Transparent };
                blend.Positions = new[] { 0f, 0.5f, 1f };
                line.InterpolationColors = blend;
                g.FillRectangle(line, x, height * 0.12f, 1f, height * 0.76f);
            }
        }

        private void DrawMessage(Graphics g, RectangleF area, float time)
        {
            float seconds = time % 10f;
            string[] lines =
            {
                "遇见你之前，",
                "我以为生活只是日复一日。",
                "遇见你以后，",
                "平凡的日子开始有了期待，",
                "一盏灯、一顿饭、一次相视而笑，",
                "都成了我最珍贵的幸福。",
                "谢谢你走进我的生命，",
                "也谢谢你愿意陪我走过往后的岁月。",
                "我不敢许诺每一天都完美，",
                "但我会认真爱你、珍惜你、陪伴你。",
                "往后余生，",
                "愿清晨醒来是你，",
                "岁月尽头，依然是你。",
                "我爱你。"
            };

            float baseSize = Math.Max(17f, Math.Min(29f, Height / 39f));
            float lineHeight = Math.Max(29f, Math.Min(47f, Height / 24f));
            float totalHeight = lineHeight * lines.Length + lineHeight * 0.9f;
            float y = area.Top + Math.Max(0, (area.Height - totalHeight) / 2f);

            string bodyFamily = FindFontFamily(new[] { "Microsoft YaHei UI", "微软雅黑", "Microsoft YaHei", "SimSun" });
            string accentFamily = FindFontFamily(new[] { "STXingkai", "华文行楷", "FZShuTi", "方正舒体", bodyFamily });

            using (Font bodyFont = new Font(bodyFamily, baseSize, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Font emphasisFont = new Font(bodyFamily, baseSize + 0.5f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Font finalFont = new Font(accentFamily, baseSize * 1.62f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (StringFormat format = new StringFormat(StringFormat.GenericTypographic))
            {
                format.FormatFlags |= StringFormatFlags.NoWrap;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == 2 || i == 6 || i == 8 || i == 10 || i == 13) y += lineHeight * 0.22f;
                    float revealStart = i <= 9 ? 0.38f + i * 0.36f :
                                        i <= 12 ? 4.12f + (i - 10) * 0.47f : 7.62f;
                    float reveal = Clamp((seconds - revealStart) / (i == 13 ? 0.72f : 0.62f), 0f, 1f);
                    float eased = 1f - (float)Math.Pow(1f - reveal, 3f);
                    int alpha = (int)(235 * eased);
                    float x = area.Left + (1f - eased) * 18f;

                    bool keyLine = i == 0 || i == 2 || i == 10;
                    bool finalLine = i == lines.Length - 1;
                    Color textColor = finalLine ? Color.FromArgb(alpha, 255, 121, 178) :
                                      keyLine ? Color.FromArgb(alpha, 220, 205, 239) :
                                      Color.FromArgb(alpha, 235, 232, 238);
                    Font font = finalLine ? finalFont : (keyLine ? emphasisFont : bodyFont);
                    float finalHit = finalLine
                        ? (float)Math.Exp(-Math.Pow((seconds - 4.45f) / 0.30f, 2.0))
                        : 0f;
                    if (finalLine && reveal > 0.02f)
                    {
                        int glowAlpha = (int)((28 + 92 * finalHit) * eased);
                        using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 255, 54, 143)))
                        {
                            g.DrawString(lines[i], font, glowBrush, new PointF(x - 2f, y), format);
                            g.DrawString(lines[i], font, glowBrush, new PointF(x + 2f, y), format);
                            g.DrawString(lines[i], font, glowBrush, new PointF(x, y - 2f), format);
                            g.DrawString(lines[i], font, glowBrush, new PointF(x, y + 2f), format);
                        }
                    }
                    using (SolidBrush brush = new SolidBrush(textColor))
                        g.DrawString(lines[i], font, brush, new PointF(x, y), format);

                    if (finalLine && reveal > 0.1f)
                    {
                        using (Pen underline = new Pen(Color.FromArgb((int)((105 + 90 * finalHit) * eased), 255, 78, 151), 1.1f + finalHit * 0.9f))
                            g.DrawLine(underline, x, y + baseSize * 2.08f,
                                       x + area.Width * 0.31f * eased, y + baseSize * 2.08f);
                    }
                    y += finalLine ? lineHeight * 1.35f : lineHeight;
                }
            }
        }

        private void DrawParticleHeart(Graphics g, RectangleF area, float time)
        {
            float centerX = area.Left + area.Width * 0.50f;
            float centerY = area.Height * 0.45f;
            float scale = Math.Min(area.Width * 0.32f, area.Height * 0.34f);
            float seconds = time % 10f;
            float firstBeat = (float)Math.Exp(-Math.Pow((seconds - 4.22f) / 0.18f, 2.0));
            float secondBeat = 0f;
            float pulse = 1f + 0.010f * (float)Math.Sin(time * 2.05f) +
                          firstBeat * 0.075f + secondBeat * 0.040f;

            float poolLeft = area.Left + area.Width * 0.11f;
            float poolWidth = area.Width * 0.78f;
            float poolY = Height * 0.84f;
            DrawPurplePool(g, poolLeft, poolY, poolWidth, Height * 0.115f, time);

            DrawRisingSparks(g, poolLeft, poolY, poolWidth, centerY + scale * 0.72f, time);

            SolidBrush[] brushes = new SolidBrush[HeartColors.Length];
            for (int i = 0; i < HeartColors.Length; i++)
                brushes[i] = new SolidBrush(Color.FromArgb(i == 7 ? 248 : 228, HeartColors[i]));

            float spinElapsed = Math.Max(0f, seconds - 5.20f);
            float spinEase = SmoothStep(5.20f, 5.75f, seconds);
            float viewYaw = 0.24f + spinElapsed * 1.32f * spinEase;
            const float viewPitch = -0.075f;
            float cosYaw = (float)Math.Cos(viewYaw);
            float sinYaw = (float)Math.Sin(viewYaw);
            float cosPitch = (float)Math.Cos(viewPitch);
            float sinPitch = (float)Math.Sin(viewPitch);

            try
            {
                foreach (Particle particle in heartParticles)
                {
                    float arrival;
                    if (particle.Edge)
                    {
                        float edgeProgress = Clamp(particle.Activation / 0.53f, 0f, 1f);
                        arrival = 0.30f + edgeProgress * 1.30f;
                    }
                    else
                    {
                        float fillProgress = Clamp((particle.Activation - 0.475f) / 0.525f, 0f, 1f);
                        arrival = 1.48f + fillProgress * 2.64f;
                    }
                    float formation = SmoothStep(arrival - 0.58f, arrival, seconds);
                    if (formation <= 0.002f) continue;

                    // The heart remains front-facing. Only a restrained breathing
                    // motion and surface shimmer remain after formation.
                    float ru = particle.U * (1f + 0.008f *
                               (float)Math.Sin(time * 1.17f + particle.Phase * 0.07f));
                    float rv = particle.V * (1f + 0.007f *
                               (float)Math.Cos(time * 1.36f + particle.Phase * 0.05f));
                    rv += (float)Math.Pow(Math.Abs(ru), 3.1) *
                          (float)Math.Sin(time * 0.76f + particle.Phase * 0.22f) * 0.012f;

                    // After formation, the volumetric heart smoothly rotates around
                    // its vertical axis while the greeting remains front-facing.
                    float viewX = ru * cosYaw + particle.Z * sinYaw;
                    float rotatedDepth = -ru * sinYaw + particle.Z * cosYaw;
                    float viewY = rv * cosPitch - rotatedDepth * sinPitch;
                    float finalZ = rv * sinPitch + rotatedDepth * cosPitch;
                    float perspective = Clamp(1f / (1f - finalZ * 0.34f), 0.68f, 1.45f);
                    float surfaceX = (float)Math.Sin(viewY * 8.5f + time * 1.15f + particle.Phase) * scale * 0.004f;
                    float surfaceY = (float)Math.Cos(viewX * 9.0f - time * 0.82f + particle.Phase) * scale * 0.003f;
                    float x = centerX + viewX * scale * pulse * perspective + surfaceX;
                    float y = centerY + viewY * scale * pulse * perspective + surfaceY;

                    if (formation < 0.999f)
                    {
                        float fly = 1f - (float)Math.Pow(1f - formation, 3.0);
                        float sourceX = centerX + particle.SourceU * poolWidth * 0.43f;
                        float sourceY = poolY + (particle.SourceV - 0.5f) * Height * 0.075f;
                        x = Lerp(sourceX, x, fly) +
                            (float)Math.Sin(fly * Math.PI + particle.Phase) * scale * 0.032f * (1f - fly);
                        y = Lerp(sourceY, y, fly) -
                            (float)Math.Sin(fly * Math.PI) * scale * 0.045f;
                    }

                    float shimmer = 0.70f + 0.30f *
                                    (float)((Math.Sin(time * 4.1f + particle.Phase) + 1.0) * 0.5);
                    float depthLight = Clamp((finalZ + 0.92f) / 1.84f, 0f, 1f);
                    float surfaceRadius = (float)Math.Sqrt((ru * ru) / 1.15f + (rv * rv) / 1.18f);
                    float bulgeLight = (float)Math.Pow(Math.Max(0f, 1f - Math.Min(1f, surfaceRadius)), 0.48f);
                    float convexLight = Clamp(depthLight * 0.58f + bulgeLight * 0.60f - viewX * 0.035f, 0f, 1f);
                    float depthScale = 0.54f + depthLight * 0.60f + bulgeLight * 0.25f;
                    int depthShift = convexLight > 0.84f ? 3 :
                                     convexLight > 0.68f ? 2 :
                                     convexLight > 0.52f ? 1 :
                                     convexLight < 0.23f ? -2 :
                                     convexLight < 0.39f ? -1 : 0;
                    int ci = Math.Max(0, Math.Min(HeartColors.Length - 1,
                                      particle.ColorIndex + depthShift));
                    float dotSize = (particle.Edge ? 1.22f : 0.72f) +
                                    particle.Size * (particle.Edge ? 0.64f : 0.50f) * depthScale;
                    dotSize *= 0.84f + shimmer * 0.22f;

                    if (formation < 0.985f)
                    {
                        float colorShift = SmoothStep(0.18f, 0.84f, formation);
                        int purpleIndex = particle.Depth > 0.72f ? 4 : 3;
                        Color flyingColor = BlendColor(PurpleColors[purpleIndex], HeartColors[ci], colorShift);
                        int flyingAlpha = (int)(65 + 180 * SmoothStep(0.04f, 0.72f, formation));
                        float flyingSize = Math.Max(0.72f, dotSize * (0.66f + formation * 0.34f));
                        if (((int)(particle.Phase * 100f)) % 47 == 0 && formation > 0.34f)
                        {
                            float halo = flyingSize * 3.2f;
                            using (SolidBrush glow = new SolidBrush(Color.FromArgb((int)(28 * formation), flyingColor)))
                                g.FillEllipse(glow, x - halo * 0.5f, y - halo * 0.5f, halo, halo);
                        }
                        using (SolidBrush flyingBrush = new SolidBrush(Color.FromArgb(flyingAlpha, flyingColor)))
                            g.FillEllipse(flyingBrush, x - flyingSize * 0.5f, y - flyingSize * 0.5f,
                                          flyingSize, flyingSize);
                    }
                    else
                    {
                        g.FillEllipse(brushes[ci], x - dotSize * 0.5f, y - dotSize * 0.5f,
                                      dotSize, dotSize);
                        if (convexLight > 0.82f && ((int)(particle.Phase * 1000f)) % 89 == 0)
                        {
                            float halo = dotSize * 3.6f;
                            using (SolidBrush hotSpot = new SolidBrush(Color.FromArgb(34, 255, 184, 217)))
                                g.FillEllipse(hotSpot, x - halo * 0.5f, y - halo * 0.5f, halo, halo);
                        }
                        if (particle.Edge && ci >= 5 && particle.Size > 2.45f && shimmer > 0.92f)
                        {
                            using (Pen glint = new Pen(Color.FromArgb(105, 255, 228, 243), 0.75f))
                            {
                                float ray = 2.6f + particle.Size;
                                g.DrawLine(glint, x - ray, y, x + ray, y);
                                g.DrawLine(glint, x, y - ray, x, y + ray);
                            }
                        }
                    }
                }
            }
            finally
            {
                for (int i = 0; i < HeartColors.Length; i++) brushes[i].Dispose();
            }

            float shockAge = seconds - 4.15f;
            if (shockAge >= 0f && shockAge <= 1.18f)
            {
                float ring = shockAge / 1.18f;
                int ringAlpha = (int)(150 * (1f - ring) * (1f - ring));
                using (GraphicsPath ringPath = CreateHeartPath(centerX, centerY,
                                               scale * pulse * (1f + ring * 0.115f)))
                using (Pen ringPen = new Pen(Color.FromArgb(ringAlpha, 255, 103, 175),
                                             1.1f + ring * 1.1f))
                    g.DrawPath(ringPen, ringPath);
            }

            float greetingReveal = SmoothStep(4.30f, 4.88f, seconds);
            if (greetingReveal > 0.002f)
            {
                string greetingFamily = FindFontFamily(new[]
                    { "STXingkai", "华文行楷", "KaiTi", "楷体", "Microsoft YaHei" });
                float greetingSize = Math.Max(25f, scale * 0.175f);
                float greetingRise = (1f - greetingReveal) * 12f;
                RectangleF greetingArea = new RectangleF(
                    centerX - scale * 0.72f,
                    centerY - greetingSize * 0.64f + greetingRise,
                    scale * 1.44f,
                    greetingSize * 1.65f);
                using (Font greetingFont = new Font(greetingFamily, greetingSize,
                                                     FontStyle.Bold, GraphicsUnit.Pixel))
                using (StringFormat greetingFormat = new StringFormat(StringFormat.GenericTypographic))
                {
                    greetingFormat.Alignment = StringAlignment.Center;
                    greetingFormat.LineAlignment = StringAlignment.Center;
                    int textAlpha = (int)(248 * greetingReveal);
                    using (SolidBrush shadow = new SolidBrush(Color.FromArgb((int)(170 * greetingReveal), 9, 3, 12)))
                        g.DrawString("七夕快乐", greetingFont, shadow,
                                     new RectangleF(greetingArea.X + 2f, greetingArea.Y + 3f,
                                                    greetingArea.Width, greetingArea.Height), greetingFormat);
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb((int)(92 * greetingReveal), 255, 85, 158)))
                    {
                        g.DrawString("七夕快乐", greetingFont, glow,
                                     new RectangleF(greetingArea.X - 2f, greetingArea.Y,
                                                    greetingArea.Width, greetingArea.Height), greetingFormat);
                        g.DrawString("七夕快乐", greetingFont, glow,
                                     new RectangleF(greetingArea.X + 2f, greetingArea.Y,
                                                    greetingArea.Width, greetingArea.Height), greetingFormat);
                    }
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(textAlpha, 255, 235, 245)))
                        g.DrawString("七夕快乐", greetingFont, textBrush, greetingArea, greetingFormat);
                }
            }
        }

        private void DrawParticleHeartLegacy(Graphics g, RectangleF area, float time)
        {
            float centerX = area.Left + area.Width * 0.535f;
            float centerY = area.Height * 0.405f;
            float scale = Math.Min(area.Width * 0.315f, area.Height * 0.305f);
            float pulse = 1f + 0.014f * (float)Math.Sin(time * 2.20f);
            float seconds = time % 10f;
            // The rising stream and the heart share the same growth clock:
            // more particles rise, and more pink particles stay in the heart.
            float density = 0.025f + 0.975f * SmoothStep(0.20f, 5.25f, seconds);

            float poolLeft = area.Left + area.Width * 0.055f;
            float poolWidth = area.Width * 0.89f;
            float poolY = Height * 0.835f;
            DrawPurplePool(g, poolLeft, poolY, poolWidth, Height * 0.115f, time);

            Pen[] pens = new Pen[HeartColors.Length];
            SolidBrush[] brushes = new SolidBrush[HeartColors.Length];
            for (int i = 0; i < HeartColors.Length; i++)
            {
                pens[i] = new Pen(Color.FromArgb(i == 7 ? 248 : 232, HeartColors[i]), i >= 5 ? 1.80f : 1.45f);
                pens[i].StartCap = LineCap.Round;
                pens[i].EndCap = LineCap.Round;
                brushes[i] = new SolidBrush(Color.FromArgb(i == 7 ? 245 : 218, HeartColors[i]));
            }

            try
            {
                foreach (Particle particle in heartParticles)
                {
                    float threshold = density + (particle.Edge ? 0.035f : 0f);
                    bool active = particle.Activation <= threshold;
                    if (!active) continue;

                    // Keep the heart facing forward. It breathes and shimmers,
                    // but it never rotates.
                    float ru = particle.U;
                    float rv = particle.V;
                    ru *= 1f + 0.018f * (float)Math.Sin(time * 1.17f + particle.Phase * 0.07f);
                    rv *= 1f + 0.014f * (float)Math.Cos(time * 1.36f + particle.Phase * 0.05f);
                    rv += (float)Math.Pow(Math.Abs(ru), 3.1) *
                          (float)Math.Sin(time * 0.76f + particle.Phase * 0.22f) * 0.033f;

                    float x3 = ru;
                    float y3 = rv;
                    float finalZ = particle.Z;
                    float perspective = Clamp(1f / (1f - finalZ * 0.24f), 0.80f, 1.28f);
                    float surfaceX = (float)Math.Sin(y3 * 8.5f + time * 1.15f + particle.Phase) * scale * 0.009f;
                    float surfaceY = (float)Math.Cos(x3 * 9.0f - time * 0.82f + particle.Phase) * scale * 0.007f;
                    float x = centerX + x3 * scale * pulse * perspective + surfaceX;
                    float y = centerY + y3 * scale * pulse * perspective + surfaceY;

                    float formation = 1f;
                    if (seconds < 5.85f)
                    {
                        float start = 0.08f + particle.Activation * 4.78f;
                        formation = SmoothStep(start, start + 0.62f, seconds);
                    }

                    if (formation < 0.999f)
                    {
                        float fly = 1f - (float)Math.Pow(1f - formation, 3.0);
                        float sourceX = centerX + particle.SourceU * poolWidth * 0.43f;
                        float sourceY = poolY + (particle.SourceV - 0.5f) * Height * 0.075f;
                        x = Lerp(sourceX, x, fly) + (float)Math.Sin(fly * Math.PI + particle.Phase) * scale * 0.055f * (1f - fly);
                        y = Lerp(sourceY, y, fly) - (float)Math.Sin(fly * Math.PI) * scale * 0.18f;
                    }

                    float shimmer = 0.70f + 0.30f * (float)((Math.Sin(time * 4.1f + particle.Phase) + 1.0) * 0.5);
                    float depthLight = Clamp((finalZ + 0.62f) / 1.24f, 0f, 1f);
                    float depthScale = 0.72f + depthLight * 0.52f;
                    float length = particle.Size * (1.62f + particle.Depth * 1.36f) * shimmer * depthScale;
                    float angle = particle.Angle + finalZ * 0.34f +
                                  (float)Math.Sin(time * 1.1f + particle.Phase) * 0.30f;
                    float dx = (float)Math.Cos(angle) * length;
                    float dy = (float)Math.Sin(angle) * length * 0.72f;
                    int depthShift = depthLight > 0.70f ? 1 : (depthLight < 0.30f ? -1 : 0);
                    int ci = Math.Max(0, Math.Min(HeartColors.Length - 1, particle.ColorIndex + depthShift));

                    if (particle.Depth < 0.58f)
                    {
                        float dotSize = (1.0f + particle.Size * 0.70f) * depthScale;
                        g.FillEllipse(brushes[ci], x - dotSize * 0.5f, y - dotSize * 0.5f, dotSize, dotSize * 0.82f);
                    }
                    else
                    {
                        g.DrawLine(pens[ci], x - dx * 0.45f, y - dy * 0.45f, x + dx * 0.55f, y + dy * 0.55f);
                        if (ci >= 6 && particle.Size > 2.2f)
                            g.FillEllipse(brushes[ci], x - 1.1f, y - 1.1f, 2.2f, 2.2f);
                        if (ci == 7 && depthLight > 0.68f && particle.Phase < 0.9f && shimmer > 0.90f)
                        {
                            using (Pen glint = new Pen(Color.FromArgb(120, 255, 236, 246), 0.8f))
                            {
                                float ray = 3.5f + particle.Size;
                                g.DrawLine(glint, x - ray, y, x + ray, y);
                                g.DrawLine(glint, x, y - ray, x, y + ray);
                            }
                        }
                    }
                }
            }
            finally
            {
                for (int i = 0; i < HeartColors.Length; i++)
                {
                    pens[i].Dispose();
                    brushes[i].Dispose();
                }
            }

            DrawRisingSparks(g, poolLeft, poolY, poolWidth, centerY + scale * 0.72f, time);
        }

        private void DrawPurplePool(Graphics g, float left, float y, float width, float height, float time)
        {
            float centerX = left + width * 0.5f;
            foreach (DustParticle dust in poolParticles)
            {
                // A very slow rotation keeps the purple particle bed alive without
                // turning it into a fast mechanical disc.
                float radius = (float)Math.Sqrt(dust.U * dust.U + dust.V * dust.V);
                float layerSpeed = radius < 0.34f ? -0.115f : (radius < 0.70f ? 0.145f : 0.082f);
                float gentleTurn = time * layerSpeed;
                float cosTurn = (float)Math.Cos(gentleTurn);
                float sinTurn = (float)Math.Sin(gentleTurn);
                float rotatedU = dust.U * cosTurn - dust.V * sinTurn;
                float rotatedV = dust.U * sinTurn + dust.V * cosTurn;
                float edgeSoftness = 1f - 0.28f * Math.Abs(rotatedV);
                float drift = (float)Math.Sin(time * 1.15f + dust.Phase * 1.7f) * width * 0.0035f;
                float ripple = (float)Math.Sin(rotatedU * 25f + time * 3.1f + dust.Phase) *
                               (0.65f + 1.65f * (1f - Math.Abs(rotatedV)));
                float x = centerX + rotatedU * width * 0.50f + drift;
                float py = y + rotatedV * height * 0.48f + ripple;
                float twinkle = 0.34f + 0.66f * (float)((Math.Sin(time * 4.2f + dust.Phase) + 1.0) / 2.0);
                float size = Math.Max(0.68f, dust.Size * (0.58f + twinkle * 0.78f));
                int brightIndex = Math.Min(PurpleColors.Length - 1, dust.ColorIndex + 1);
                Color color = PurpleColors[brightIndex];
                int alpha = (int)((92f + 163f * twinkle) * edgeSoftness);

                if (dust.ColorIndex >= 4 && dust.Size > 2.45f && twinkle > 0.72f)
                {
                    float halo = size * 2.8f;
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(24, 224, 199, 255)))
                        g.FillEllipse(glow, x - halo * 0.5f, py - halo * 0.26f, halo, halo * 0.52f);
                }
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, color)))
                    g.FillEllipse(brush, x - size * 0.5f, py - size * 0.28f, size, Math.Max(0.65f, size * 0.56f));
            }
        }

        private void DrawRisingSparks(Graphics g, float left, float poolY, float width, float heartY, float time)
        {
            float seconds = time % 10f;
            float growth = 0.35f * SmoothStep(0.10f, 1.55f, seconds) +
                           0.65f * SmoothStep(1.55f, 4.12f, seconds);
            float settle = 1f - 0.44f * SmoothStep(5.10f, 6.20f, seconds);
            int count = Math.Min((int)(145 + 1130 * growth * settle), poolParticles.Count);
            float centerX = left + width * 0.5f;
            float flightHeight = Math.Max(90f, (poolY - heartY) * 1.02f);
            for (int i = 0; i < count; i++)
            {
                DustParticle dust = poolParticles[i];
                float sparkCycle = (time * (0.235f + dust.Lift / 920f) + dust.Phase * 0.071f) % 1f;
                float lateral = dust.U * width * (0.015f + 0.205f * sparkCycle);
                float flutter = (float)Math.Sin(time * 2.1f + dust.Phase * 2.4f) * (1.5f + 7f * sparkCycle);
                float x = centerX + lateral + flutter;
                float heightVariation = 0.58f + dust.Lift / 270f;
                // A one-way path: rise continuously, then fade out near the top.
                // The next cycle starts again at the particle bed; nothing falls.
                float y = poolY - sparkCycle * flightHeight * heightVariation;
                float fadeIn = SmoothStep(0.00f, 0.08f, sparkCycle);
                float fadeOut = 1f - SmoothStep(0.72f, 1.00f, sparkCycle);
                float fade = fadeIn * fadeOut;
                bool largeSpark = i % 43 == 0;
                bool mediumSpark = !largeSpark && i % 7 == 0;
                float sizeBase = largeSpark ? 2.55f : (mediumSpark ? 1.48f : 0.68f);
                float size = Math.Max(0.62f, (sizeBase + dust.Size * 0.32f) * (0.72f + 0.38f * fade));
                int colorIndex = i % 8 == 0 ? 4 : (i % 3 == 0 ? 3 : 2);
                Color purple = PurpleColors[colorIndex];
                Color pink = HeartColors[i % 9 == 0 ? 5 : 4];
                float colorShift = SmoothStep(0.28f, 0.80f, sparkCycle);
                Color color = BlendColor(purple, pink, colorShift);
                int alpha = (int)(18 + 226 * fade);

                if (largeSpark && fade > 0.35f)
                {
                    float halo = size * 3.1f;
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb((int)(30 * fade), 236, 221, 255)))
                        g.FillEllipse(glow, x - halo * 0.5f, y - halo * 0.5f, halo, halo);
                }
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, color)))
                    g.FillEllipse(brush, x - size * 0.5f, y - size * 0.5f, size, size);

                if (i % 29 == 0 && sparkCycle > 0.08f && sparkCycle < 0.78f)
                {
                    using (Pen streak = new Pen(Color.FromArgb((int)(76 * fade), color), 0.85f))
                        g.DrawLine(streak, x, y + size * 0.4f, x - dust.U * 2.5f, y + 6f + size * 1.35f);
                }
            }
        }

        private static GraphicsPath CreateHeartPath(float centerX, float centerY, float scale)
        {
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[181];
            for (int i = 0; i < points.Length; i++)
            {
                double t = i / 180.0 * Math.PI * 2.0;
                float x = (float)(16.0 * Math.Pow(Math.Sin(t), 3.0) / 17.0);
                float y = (float)(-(13.0 * Math.Cos(t) - 5.0 * Math.Cos(2.0 * t) -
                                    2.0 * Math.Cos(3.0 * t) - Math.Cos(4.0 * t)) / 17.0);
                points[i] = new PointF(centerX + x * scale, centerY + y * scale);
            }
            path.AddPolygon(points);
            return path;
        }

        private static string FindFontFamily(string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                try
                {
                    using (FontFamily family = new FontFamily(candidate)) return family.Name;
                }
                catch { }
            }
            return FontFamily.GenericSansSerif.Name;
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        private static Color BlendColor(Color from, Color to, float amount)
        {
            float t = Clamp(amount, 0f, 1f);
            return Color.FromArgb(
                (int)Lerp(from.A, to.A, t),
                (int)Lerp(from.R, to.R, t),
                (int)Lerp(from.G, to.G, t),
                (int)Lerp(from.B, to.B, t));
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Clamp((value - edge0) / Math.Max(0.0001f, edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        private static float Lerp(float a, float b, float amount)
        {
            return a + (b - a) * amount;
        }
    }
}
