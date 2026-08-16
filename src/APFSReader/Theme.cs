using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace APFSReader
{
    /// <summary>
    /// The insta.host look, ported to Windows Forms.
    /// <para>
    /// The palette is taken verbatim from the design tokens in insta.host/tools/styles.css so the
    /// application and the page it is downloaded from read as the same product. WinForms has no
    /// theming engine of its own, so the colours are pushed onto the control tree by
    /// <see cref="Apply(Control)"/>, and the parts Windows draws itself - scroll bars, list headers,
    /// combo drop-downs, the title bar - are switched over with the shell's dark-mode hooks in
    /// <see cref="DarkMode"/>.
    /// </para>
    /// </summary>
    internal static class Theme
    {
        // ---- palette (insta.host design tokens) -----------------------------------------
        //   --bg:#04060f  --bg2:#0a1030  --ink:#eaf0ff  --muted:#93a0cf
        //   --cyan:#22d3ee  --violet:#a855f7  --blue:#4f74ff  --danger:#ff9d9d
        public static readonly Color Bg = FromHex(0x04060F);
        public static readonly Color Bg2 = FromHex(0x0A1030);
        public static readonly Color Ink = FromHex(0xEAF0FF);
        public static readonly Color Muted = FromHex(0x93A0CF);
        public static readonly Color Cyan = FromHex(0x22D3EE);
        public static readonly Color Violet = FromHex(0xA855F7);
        public static readonly Color Blue = FromHex(0x4F74FF);
        public static readonly Color Danger = FromHex(0xFF9D9D);
        public static readonly Color Success = FromHex(0x6EE7B7);
        public static readonly Color Warning = FromHex(0xFCD34D);

        /// <summary>Card surface - the flattened equivalent of rgba(20,28,64,.42) over --bg.</summary>
        public static readonly Color Surface = FromHex(0x0C1230);

        /// <summary>A raised surface for inputs and list bodies.</summary>
        public static readonly Color Field = FromHex(0x0A1030);

        /// <summary>Hairline borders - the flattened equivalent of rgba(140,165,255,.14).</summary>
        public static readonly Color Line = FromHex(0x1C2547);

        /// <summary>A brighter border for hover and focus.</summary>
        public static readonly Color LineBright = FromHex(0x2C3A6B);

        /// <summary>Console / log surface.</summary>
        public static readonly Color Console = FromHex(0x05070F);

        // ---- names APFS Reader's own windows were already written against ----------------
        // 1.2.0 hand-rounded this palette under its own field names before the canonical
        // token set existed here. These are aliases onto the real tokens above, not a second
        // palette, so every window now paints the exact same colours as every other tool
        // without a rewrite of MainForm.cs and AboutDialog.cs.
        public static readonly Color Background = Bg;
        public static readonly Color Elevated = Field;
        public static readonly Color SurfaceSoft = Mix(Surface, LineBright, 0.35);
        public static readonly Color Border = Line;
        public static readonly Color Text = Ink;
        public static readonly Color Accent = Cyan;
        public static readonly Color AccentHover = Violet;

        // ---- status colours -------------------------------------------------------------
        // List rows previously used dark reds and greens that were chosen for a white
        // background. These are the readable equivalents on the dark ground.

        /// <summary>Missing, failed, or needs manual work.</summary>
        public static readonly Color StatusBad = Danger;

        /// <summary>Present, healthy, or handled automatically.</summary>
        public static readonly Color StatusGood = Success;

        /// <summary>In use, in progress, or needs attention but is not an error.</summary>
        public static readonly Color StatusWarn = Warning;

        /// <summary>Not applicable, absent, or deliberately ignored.</summary>
        public static readonly Color StatusIdle = Mix(Muted, Bg, 0.35);

        // ---- fonts ----------------------------------------------------------------------
        // The site uses Space Grotesk, Inter and JetBrains Mono. Those are web fonts and will not
        // normally be present on a technician's machine, so each one degrades to the best Windows
        // equivalent that is actually installed.
        private static readonly string[] DisplayStack = { "Space Grotesk", "Segoe UI Variable Display", "Segoe UI Semibold", "Segoe UI" };
        private static readonly string[] BodyStack = { "Inter", "Segoe UI Variable Text", "Segoe UI" };
        private static readonly string[] MonoStack = { "JetBrains Mono", "Cascadia Mono", "Consolas", "Courier New" };

        private static string _display, _body, _mono;

        public static string DisplayFamily => _display ??= FirstInstalled(DisplayStack);
        public static string BodyFamily => _body ??= FirstInstalled(BodyStack);
        public static string MonoFamily => _mono ??= FirstInstalled(MonoStack);

        private static Font _ui, _uiBold, _uiSmall, _h1, _h2, _kicker, _monoSmall;

        public static Font Ui => _ui ??= new Font(BodyFamily, 9f);
        public static Font UiBold => _uiBold ??= new Font(BodyFamily, 9f, FontStyle.Bold);
        public static Font UiSmall => _uiSmall ??= new Font(BodyFamily, 8.25f);
        public static Font H1 => _h1 ??= new Font(DisplayFamily, 15f, FontStyle.Bold);
        public static Font H2 => _h2 ??= new Font(DisplayFamily, 10.5f, FontStyle.Bold);

        /// <summary>The uppercase, letter-spaced label the site sets above a heading.</summary>
        public static Font Kicker => _kicker ??= new Font(MonoFamily, 7.5f, FontStyle.Bold);
        public static Font MonoSmall => _monoSmall ??= new Font(MonoFamily, 8.75f);

        private static string FirstInstalled(string[] candidates)
        {
            try
            {
                using (var installed = new InstalledFontCollection())
                {
                    foreach (var name in candidates)
                        foreach (var family in installed.Families)
                            if (string.Equals(family.Name, name, StringComparison.OrdinalIgnoreCase))
                                return family.Name;
                }
            }
            catch { }
            return candidates[candidates.Length - 1];
        }

        public static Color FromHex(int rgb) =>
            Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);

        /// <summary>Mixes two colours; <paramref name="amount"/> of 0 returns <paramref name="a"/>.</summary>
        public static Color Mix(Color a, Color b, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                (int)Math.Round(a.R + (b.R - a.R) * amount),
                (int)Math.Round(a.G + (b.G - a.G) * amount),
                (int)Math.Round(a.B + (b.B - a.B) * amount));
        }

        /// <summary>The cyan-to-violet gradient the site uses for the brand mark and card rules.</summary>
        public static LinearGradientBrush BrandGradient(Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) bounds = new Rectangle(0, 0, 1, 1);
            return new LinearGradientBrush(bounds, Cyan, Violet, LinearGradientMode.Horizontal);
        }

        /// <summary>A rounded rectangle path; used by the cards and the header bar.</summary>
        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            if (r.Width <= 0 || r.Height <= 0) { path.AddRectangle(r); return path; }

            var d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // =================================================================================
        // Applying the theme
        // =================================================================================

        /// <summary>Put this in a control's <see cref="Control.Tag"/> to leave it alone.</summary>
        public const string SkipTag = "theme:skip";

        /// <summary>Marks a button as the primary action on its screen.</summary>
        public const string PrimaryTag = "theme:primary";

        /// <summary>Marks a button as destructive or cancelling.</summary>
        public const string DangerTag = "theme:danger";

        /// <summary>
        /// Walks a control tree and restyles every control on it. Safe to call more than once, and
        /// safe to call before the handles exist - anything needing a window handle is deferred to
        /// <see cref="Control.HandleCreated"/>.
        /// </summary>
        public static void Apply(Control root)
        {
            if (root == null) return;
            Style(root);
            foreach (Control child in root.Controls) Apply(child);
        }

        /// <summary>Restyles a single control. Its children are not touched.</summary>
        public static void Style(Control c)
        {
            if (c == null) return;
            if (c.Tag as string == SkipTag) return;

            // Order matters: a derived control has to be matched before the type it derives from,
            // or the compiler reports the later case as unreachable.
            switch (c)
            {
                case Form form:
                    form.BackColor = Bg;
                    form.ForeColor = Ink;
                    DarkMode.UseDarkTitleBar(form);
                    break;

                case Button button:
                    StyleButton(button);
                    break;

                case CheckBox check:
                    check.BackColor = Color.Transparent;
                    check.ForeColor = Ink;
                    check.FlatStyle = FlatStyle.Flat;
                    check.FlatAppearance.BorderSize = 0;
                    check.FlatAppearance.CheckedBackColor = Color.Transparent;
                    check.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    break;

                case RadioButton radio:
                    radio.BackColor = Color.Transparent;
                    radio.ForeColor = Ink;
                    radio.FlatStyle = FlatStyle.Flat;
                    radio.FlatAppearance.BorderSize = 0;
                    radio.FlatAppearance.CheckedBackColor = Color.Transparent;
                    radio.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    break;

                case LinkLabel link:
                    link.BackColor = Color.Transparent;
                    link.ForeColor = Muted;
                    link.LinkColor = Cyan;
                    link.ActiveLinkColor = Violet;
                    link.VisitedLinkColor = Cyan;
                    link.DisabledLinkColor = Mix(Muted, Bg, 0.5);
                    link.LinkBehavior = LinkBehavior.HoverUnderline;
                    break;

                case Label label:
                    label.BackColor = Color.Transparent;
                    // Explanatory paragraphs go muted so the eye lands on the controls instead.
                    // Anything already given a deliberate colour keeps it.
                    if (label.ForeColor == SystemColors.ControlText)
                        label.ForeColor = Muted;
                    else if (label.ForeColor == SystemColors.GrayText)
                        label.ForeColor = Mix(Muted, Bg, 0.3);
                    break;

                case RichTextBox rich:
                    rich.BackColor = Console;
                    if (rich.ForeColor == SystemColors.WindowText) rich.ForeColor = Ink;
                    rich.BorderStyle = BorderStyle.None;
                    DarkMode.Attach(rich, DarkMode.ExplorerTheme);
                    break;

                case TextBox text:
                    text.BackColor = text.ReadOnly ? Console : Field;
                    text.ForeColor = text.ReadOnly ? Mix(Ink, Muted, 0.35) : Ink;
                    text.BorderStyle = BorderStyle.FixedSingle;
                    DarkMode.Attach(text, DarkMode.CfdTheme);
                    break;

                case ComboBox combo:
                    combo.BackColor = Field;
                    combo.ForeColor = Ink;
                    combo.FlatStyle = FlatStyle.Flat;
                    DarkMode.Attach(combo, DarkMode.CfdTheme);
                    break;

                case CheckedListBox checkedList:
                    checkedList.BackColor = Field;
                    checkedList.ForeColor = Ink;
                    checkedList.BorderStyle = BorderStyle.FixedSingle;
                    DarkMode.Attach(checkedList, DarkMode.ExplorerTheme);
                    break;

                case ListBox list:
                    list.BackColor = Field;
                    list.ForeColor = Ink;
                    list.BorderStyle = BorderStyle.FixedSingle;
                    DarkMode.Attach(list, DarkMode.ExplorerTheme);
                    break;

                case ListView listView:
                    listView.BackColor = Field;
                    listView.ForeColor = Ink;
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    // Grid lines are drawn by the OS in a fixed light colour that cannot be
                    // overridden, and they streak badly on a dark ground, so they come off.
                    listView.GridLines = false;
                    DarkMode.AttachListView(listView);
                    break;

                case TreeView tree:
                    tree.BackColor = Field;
                    tree.ForeColor = Ink;
                    tree.BorderStyle = BorderStyle.FixedSingle;
                    tree.LineColor = Line;
                    DarkMode.Attach(tree, DarkMode.ExplorerTheme);
                    break;

                case ProgressBar bar:
                    // A native progress bar ignores BackColor/ForeColor while visual styles are on.
                    // AccentProgressBar replaces it outright; anything still native at least gets
                    // the dark shell theme rather than a bright white trough.
                    DarkMode.Attach(bar, DarkMode.ExplorerTheme);
                    break;

                case ToolStrip strip:
                    strip.Renderer = ToolStripRenderer;
                    strip.BackColor = Surface;
                    strip.ForeColor = Ink;
                    break;

                case TabControl tabs:
                    tabs.BackColor = Bg;
                    tabs.ForeColor = Ink;
                    break;

                case GroupBox group:
                    StyleGroupBox(group);
                    break;

                // TabPage, TableLayoutPanel and FlowLayoutPanel all derive from Panel, so they are
                // matched first.
                case TabPage page:
                    page.BackColor = Bg;
                    page.ForeColor = Ink;
                    break;

                case TableLayoutPanel table:
                    table.BackColor = Color.Transparent;
                    table.ForeColor = Ink;
                    break;

                case FlowLayoutPanel flow:
                    flow.BackColor = Color.Transparent;
                    flow.ForeColor = Ink;
                    break;

                case Panel panel:
                    // A panel that has deliberately been given a colour keeps it, so a card surface
                    // set by the caller survives a re-theme.
                    if (panel.BackColor == SystemColors.Control) panel.BackColor = Bg;
                    panel.ForeColor = Ink;
                    break;

                case UserControl user:
                    user.BackColor = Bg;
                    user.ForeColor = Ink;
                    break;

                default:
                    if (c.BackColor == SystemColors.Control) c.BackColor = Bg;
                    if (c.ForeColor == SystemColors.ControlText) c.ForeColor = Ink;
                    break;
            }
        }

        // ---- buttons --------------------------------------------------------------------

        public static Button MakePrimary(Button b)
        {
            if (b == null) return null;
            b.Tag = PrimaryTag;
            StyleButton(b);
            return b;
        }

        public static Button MakeDanger(Button b)
        {
            if (b == null) return null;
            b.Tag = DangerTag;
            StyleButton(b);
            return b;
        }

        private static void StyleButton(Button b)
        {
            var kind = b.Tag as string;
            var primary = kind == PrimaryTag;
            var danger = kind == DangerTag;

            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.Font = primary ? UiBold : Ui;

            var accent = danger ? Danger : Cyan;

            if (primary)
            {
                b.BackColor = Mix(Bg, accent, 0.20);
                b.ForeColor = accent;
                b.FlatAppearance.BorderColor = Mix(Bg, accent, 0.55);
                b.FlatAppearance.MouseOverBackColor = Mix(Bg, accent, 0.32);
                b.FlatAppearance.MouseDownBackColor = Mix(Bg, accent, 0.42);
            }
            else if (danger)
            {
                b.BackColor = Surface;
                b.ForeColor = Danger;
                b.FlatAppearance.BorderColor = Mix(Line, Danger, 0.45);
                b.FlatAppearance.MouseOverBackColor = Mix(Surface, Danger, 0.18);
                b.FlatAppearance.MouseDownBackColor = Mix(Surface, Danger, 0.26);
            }
            else
            {
                b.BackColor = Surface;
                b.ForeColor = Ink;
                b.FlatAppearance.BorderColor = Line;
                b.FlatAppearance.MouseOverBackColor = Mix(Surface, Blue, 0.22);
                b.FlatAppearance.MouseDownBackColor = Mix(Surface, Blue, 0.32);
            }

            b.FlatAppearance.BorderSize = 1;

            // A flat button that Windows merely greys out is hard to read on a dark ground, so the
            // disabled state is dimmed explicitly and kept in step with Enabled.
            b.EnabledChanged -= OnButtonEnabledChanged;
            b.EnabledChanged += OnButtonEnabledChanged;
            b.Paint -= PaintDisabledButton;
            b.Paint += PaintDisabledButton;
            ApplyButtonCursor(b);
        }

        private static void OnButtonEnabledChanged(object sender, EventArgs e)
        {
            if (sender is Button b) { ApplyButtonCursor(b); b.Invalidate(); }
        }

        private static void ApplyButtonCursor(Button b) =>
            b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;

        private static void PaintDisabledButton(object sender, PaintEventArgs e)
        {
            var b = (Button)sender;
            if (b.Enabled) return;

            // Repaint the face and caption at a low contrast so a disabled button reads as
            // "not yet" rather than as an unreadable smudge.
            using (var fill = new SolidBrush(Mix(Bg, Surface, 0.5)))
                e.Graphics.FillRectangle(fill, 1, 1, b.Width - 2, b.Height - 2);
            using (var pen = new Pen(Mix(Line, Bg, 0.45)))
                e.Graphics.DrawRectangle(pen, 0, 0, b.Width - 1, b.Height - 1);
            TextRenderer.DrawText(e.Graphics, b.Text, b.Font,
                new Rectangle(0, 0, b.Width, b.Height), Mix(Muted, Bg, 0.45),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        // ---- group boxes ----------------------------------------------------------------

        /// <summary>
        /// A GroupBox always draws the raised 3D frame and its caption in the system colours, so it
        /// is painted from scratch instead: a hairline rounded card with the caption sitting on the
        /// border, which is what the .panel rule on the site produces.
        /// </summary>
        private static void StyleGroupBox(GroupBox g)
        {
            g.BackColor = Color.Transparent;
            g.ForeColor = Ink;
            g.FlatStyle = FlatStyle.Flat;
            g.Font = UiBold;
            g.Paint -= PaintGroupBox;
            g.Paint += PaintGroupBox;
            g.Invalidate();
        }

        private static void PaintGroupBox(object sender, PaintEventArgs e)
        {
            var g = (GroupBox)sender;
            var gfx = e.Graphics;
            var backdrop = g.Parent != null ? g.Parent.BackColor : Bg;
            if (backdrop == Color.Transparent) backdrop = Bg;

            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.Clear(backdrop);

            var caption = g.Text ?? string.Empty;
            var captionSize = caption.Length == 0
                ? SizeF.Empty
                : gfx.MeasureString(caption, g.Font);
            var top = caption.Length == 0 ? 0 : (int)(captionSize.Height / 2);

            var body = new Rectangle(0, top, g.Width - 1, g.Height - top - 1);
            if (body.Width > 0 && body.Height > 0)
            {
                using (var path = RoundedRect(body, 10))
                using (var fill = new SolidBrush(Surface))
                using (var pen = new Pen(Line))
                {
                    gfx.FillPath(fill, path);
                    gfx.DrawPath(pen, path);
                }
            }

            if (caption.Length == 0) return;

            // Punch a gap in the border so the caption is not struck through, then draw it.
            const int textX = 14;
            using (var clear = new SolidBrush(backdrop))
                gfx.FillRectangle(clear, new RectangleF(textX - 5, top - 1, captionSize.Width + 10, 3));

            using (var text = new SolidBrush(Cyan))
                gfx.DrawString(caption, g.Font, text, textX, 0);
        }

        // ---- tool strips ----------------------------------------------------------------

        private static ToolStripRenderer _renderer;
        public static ToolStripRenderer ToolStripRenderer => _renderer ??= new InstaHostToolStripRenderer();

        private sealed class InstaHostColours : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => Surface;
            public override Color ToolStripGradientMiddle => Surface;
            public override Color ToolStripGradientEnd => Surface;
            public override Color ToolStripContentPanelGradientBegin => Surface;
            public override Color ToolStripContentPanelGradientEnd => Surface;
            public override Color ToolStripPanelGradientBegin => Surface;
            public override Color ToolStripPanelGradientEnd => Surface;
            public override Color StatusStripGradientBegin => Surface;
            public override Color StatusStripGradientEnd => Surface;
            public override Color MenuStripGradientBegin => Surface;
            public override Color MenuStripGradientEnd => Surface;
            public override Color ToolStripBorder => Line;
            public override Color MenuBorder => Line;
            public override Color MenuItemBorder => Mix(Surface, Blue, 0.5);
            public override Color MenuItemSelected => Mix(Surface, Blue, 0.28);
            public override Color MenuItemSelectedGradientBegin => Mix(Surface, Blue, 0.28);
            public override Color MenuItemSelectedGradientEnd => Mix(Surface, Blue, 0.28);
            public override Color MenuItemPressedGradientBegin => Mix(Surface, Blue, 0.36);
            public override Color MenuItemPressedGradientMiddle => Mix(Surface, Blue, 0.36);
            public override Color MenuItemPressedGradientEnd => Mix(Surface, Blue, 0.36);
            public override Color ImageMarginGradientBegin => Surface;
            public override Color ImageMarginGradientMiddle => Surface;
            public override Color ImageMarginGradientEnd => Surface;
            public override Color SeparatorDark => Line;
            public override Color SeparatorLight => Line;
            public override Color CheckBackground => Mix(Surface, Cyan, 0.3);
            public override Color CheckSelectedBackground => Mix(Surface, Cyan, 0.4);
        }

        private sealed class InstaHostToolStripRenderer : ToolStripProfessionalRenderer
        {
            public InstaHostToolStripRenderer() : base(new InstaHostColours()) { RoundedEdges = false; }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Ink : Mix(Muted, Bg, 0.5);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var b = new SolidBrush(Surface))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // A single hairline along the top, matching the card rules on the site.
                using (var p = new Pen(Line))
                    e.Graphics.DrawLine(p, 0, 0, e.ToolStrip.Width, 0);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = Ink;
                base.OnRenderArrow(e);
            }
        }
    }

    /// <summary>
    /// The shell-level dark-mode switches. Windows draws scroll bars, list-view headers, combo
    /// drop-downs and the title bar itself, and none of them follow a control's BackColor. These
    /// entry points are the ones File Explorer uses to go dark, so the same controls here can
    /// follow.
    /// <para>
    /// Everything is best-effort: the uxtheme entry points are exported by ordinal only and are
    /// version-gated, so every call is guarded and a failure just leaves that piece of chrome
    /// looking native.
    /// </para>
    /// </summary>
    internal static class DarkMode
    {
        public const string ExplorerTheme = "DarkMode_Explorer";
        public const string CfdTheme = "DarkMode_CFD";
        public const string ItemsViewTheme = "DarkMode_ItemsView";

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        // uxtheme.dll exports these two by ordinal only.
        [DllImport("uxtheme.dll", EntryPoint = "#135", CharSet = CharSet.Unicode)]
        private static extern int SetPreferredAppMode(int mode);

        [DllImport("uxtheme.dll", EntryPoint = "#136", CharSet = CharSet.Unicode)]
        private static extern void FlushMenuThemes();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int LVM_GETHEADER = 0x1000 + 31;
        private const int PreferredAppModeForceDark = 2;

        private static bool _initialised;

        /// <summary>
        /// Tells the shell this process prefers dark chrome. Has to run before the first window is
        /// created, so <c>Program.Main</c> calls it first.
        /// </summary>
        public static void Init()
        {
            if (_initialised) return;
            _initialised = true;
            try
            {
                SetPreferredAppMode(PreferredAppModeForceDark);
                FlushMenuThemes();
            }
            catch
            {
                // Windows 10 before 1903 does not export the ordinal. The explicit colours still
                // apply; only the OS-drawn chrome stays light.
            }
        }

        /// <summary>Paints the title bar and window border dark.</summary>
        public static void UseDarkTitleBar(Form form)
        {
            if (form == null) return;
            if (form.IsHandleCreated) SetDarkTitleBar(form.Handle);
            else form.HandleCreated += (s, e) => SetDarkTitleBar(((Form)s).Handle);
        }

        private static void SetDarkTitleBar(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;
            var on = 1;
            try
            {
                if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref on, sizeof(int));
            }
            catch { }
        }

        /// <summary>Applies a dark window theme to a control once its handle exists.</summary>
        public static void Attach(Control c, string theme)
        {
            if (c == null) return;
            if (c.IsHandleCreated) TrySetTheme(c.Handle, theme);
            else c.HandleCreated += (s, e) => TrySetTheme(((Control)s).Handle, theme);
        }

        /// <summary>
        /// A list view needs two themes: one for the body and its scroll bars, and one for the
        /// header, which is a separate window and otherwise stays bright white.
        /// </summary>
        public static void AttachListView(ListView lv)
        {
            if (lv == null) return;
            if (lv.IsHandleCreated) ThemeListView(lv);
            else lv.HandleCreated += (s, e) => ThemeListView((ListView)s);
        }

        private static void ThemeListView(ListView lv)
        {
            TrySetTheme(lv.Handle, ExplorerTheme);
            try
            {
                var header = SendMessage(lv.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                if (header != IntPtr.Zero) TrySetTheme(header, ItemsViewTheme);
            }
            catch { }
        }

        private static void TrySetTheme(IntPtr handle, string theme)
        {
            if (handle == IntPtr.Zero) return;
            try { SetWindowTheme(handle, theme, null); } catch { }
        }
    }
}
