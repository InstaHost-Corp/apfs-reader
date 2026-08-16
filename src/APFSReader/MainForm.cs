using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APFSReader
{
    internal sealed class MainForm : Form
    {
        private static readonly Color BackgroundColor = Theme.Background;
        private static readonly Color ElevatedColor = Theme.Elevated;
        private static readonly Color SurfaceColor = Theme.Surface;
        private static readonly Color SurfaceSoftColor = Theme.SurfaceSoft;
        private static readonly Color BorderColor = Theme.Border;
        private static readonly Color TextColor = Theme.Text;
        private static readonly Color MutedColor = Theme.Muted;
        private static readonly Color AccentColor = Theme.Accent;
        private static readonly Color AccentHoverColor = Theme.AccentHover;

        private readonly TextBox source = new TextBox();
        private readonly TextBox password = new TextBox();
        private readonly NumericUpDown volume = new NumericUpDown();
        private readonly TextBox currentPath = new TextBox();
        private readonly ListView files = new ListView();
        private readonly Button browse = new Button();
        private readonly Button open = new Button();
        private readonly Button up = new Button();
        private readonly Button extract = new Button();
        private readonly Button about = new Button();
        private readonly Label status = new Label();
        private readonly ApfsBackend backend = new ApfsBackend();
        private string path = "/";

        public MainForm()
        {
            Text = "APFS Reader for Windows";
            MinimumSize = new Size(820, 560);
            Size = new Size(1020, 700);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = BackgroundColor;
            ForeColor = TextColor;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            BuildLayout();

            // Repaints scroll bars, list headers and the title bar dark, and forces every child
            // control created above onto the same palette it already used, so nothing native
            // slips back to a bright default.
            Theme.Apply(this);
        }

        private void BuildLayout()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.BackColor = BackgroundColor;
            root.Padding = new Padding(24, 18, 24, 20);
            root.ColumnCount = 1;
            root.RowCount = 5;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 142));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

            TableLayoutPanel header = new TableLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.ColumnCount = 4;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Label mark = new Label();
            mark.Text = "AP";
            mark.TextAlign = ContentAlignment.MiddleCenter;
            mark.Size = new Size(46, 46);
            mark.Margin = new Padding(0, 3, 12, 0);
            mark.BackColor = AccentColor;
            mark.ForeColor = BackgroundColor;
            mark.Font = new Font(Font.FontFamily, 12F, FontStyle.Bold);
            header.Controls.Add(mark, 0, 0);

            Panel heading = new Panel();
            heading.Dock = DockStyle.Fill;
            Label eyebrow = CreateLabel("INSTA.HOST TOOLING", 8F, FontStyle.Bold, AccentColor);
            eyebrow.Location = new Point(0, 4);
            eyebrow.AutoSize = true;
            Label title = CreateLabel("APFS Reader", 16F, FontStyle.Bold, TextColor);
            title.Location = new Point(-1, 23);
            title.AutoSize = true;
            heading.Controls.Add(eyebrow);
            heading.Controls.Add(title);
            header.Controls.Add(heading, 1, 0);

            Label mode = CreateLabel("READ ONLY", 8F, FontStyle.Bold, AccentColor);
            mode.AutoSize = false;
            mode.Size = new Size(86, 28);
            mode.TextAlign = ContentAlignment.MiddleCenter;
            mode.BackColor = SurfaceSoftColor;
            mode.Margin = new Padding(0, 10, 0, 0);
            header.Controls.Add(mode, 2, 0);

            about.Text = "About";
            about.Click += delegate { ShowAbout(); };
            StyleButton(about, false);
            about.Size = new Size(76, 28);
            about.Margin = new Padding(10, 10, 0, 0);
            header.Controls.Add(about, 3, 0);
            root.Controls.Add(header, 0, 0);

            Panel sourceCard = CreateCard();
            sourceCard.Margin = new Padding(0, 4, 0, 10);
            TableLayoutPanel sourceLayout = new TableLayoutPanel();
            sourceLayout.Dock = DockStyle.Fill;
            sourceLayout.ColumnCount = 4;
            sourceLayout.RowCount = 3;
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            sourceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            sourceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            sourceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            Label sourceHeading = CreateLabel("OPEN APFS SOURCE", 8F, FontStyle.Bold, MutedColor);
            sourceHeading.Dock = DockStyle.Fill;
            sourceHeading.TextAlign = ContentAlignment.MiddleLeft;
            sourceLayout.Controls.Add(sourceHeading, 0, 0);
            sourceLayout.SetColumnSpan(sourceHeading, 4);

            sourceLayout.Controls.Add(CreateFieldLabel("SOURCE"), 0, 1);
            source.Dock = DockStyle.Fill;
            source.Margin = new Padding(0, 4, 10, 4);
            StyleInput(source);
            browse.Text = "Browse";
            browse.Click += BrowseClick;
            StyleButton(browse, false);
            open.Text = "Open";
            open.Click += delegate { LoadFolder("/"); };
            StyleButton(open, true);
            sourceLayout.Controls.Add(source, 1, 1);
            sourceLayout.Controls.Add(browse, 2, 1);
            sourceLayout.Controls.Add(open, 3, 1);

            sourceLayout.Controls.Add(CreateFieldLabel("PASSWORD"), 0, 2);
            password.UseSystemPasswordChar = true;
            password.Dock = DockStyle.Fill;
            password.Margin = new Padding(0, 4, 10, 4);
            StyleInput(password);
            volume.Minimum = 1;
            volume.Maximum = 100;
            volume.Value = 1;
            volume.Width = 72;
            volume.Height = 30;
            volume.Margin = new Padding(8, 4, 0, 4);
            volume.BackColor = ElevatedColor;
            volume.ForeColor = TextColor;
            sourceLayout.Controls.Add(password, 1, 2);
            Label volumeLabel = CreateFieldLabel("VOLUME");
            volumeLabel.Margin = new Padding(4, 0, 0, 0);
            sourceLayout.Controls.Add(volumeLabel, 2, 2);
            sourceLayout.Controls.Add(volume, 3, 2);
            sourceCard.Controls.Add(sourceLayout);
            root.Controls.Add(sourceCard, 0, 1);

            TableLayoutPanel navigation = new TableLayoutPanel();
            navigation.Dock = DockStyle.Fill;
            navigation.Margin = new Padding(0, 2, 0, 8);
            navigation.ColumnCount = 2;
            navigation.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            navigation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            up.Text = "<  Up";
            up.Click += delegate { LoadFolder(ParentPath(path)); };
            StyleButton(up, false);
            up.Margin = new Padding(0, 2, 10, 2);
            currentPath.ReadOnly = true;
            currentPath.Dock = DockStyle.Fill;
            currentPath.Margin = new Padding(0, 2, 0, 2);
            currentPath.Text = "/";
            currentPath.Font = new Font("Consolas", 9F);
            StyleInput(currentPath);
            navigation.Controls.Add(up);
            navigation.Controls.Add(currentPath);
            root.Controls.Add(navigation, 0, 2);

            Panel fileCard = CreateCard();
            fileCard.Margin = new Padding(0);
            fileCard.Padding = new Padding(1);
            files.Dock = DockStyle.Fill;
            files.View = View.Details;
            files.FullRowSelect = true;
            files.MultiSelect = true;
            files.HideSelection = false;
            files.BorderStyle = BorderStyle.None;
            files.BackColor = SurfaceColor;
            files.ForeColor = TextColor;
            files.Font = new Font("Segoe UI", 9.25F);
            files.OwnerDraw = true;
            files.Columns.Add("NAME", 590);
            files.Columns.Add("TYPE", 120);
            files.Columns.Add("SIZE", 140, HorizontalAlignment.Right);
            files.DrawColumnHeader += DrawColumnHeader;
            files.DrawItem += delegate { };
            files.DrawSubItem += DrawSubItem;
            files.Resize += delegate { ResizeFileColumns(); };
            files.DoubleClick += FilesDoubleClick;
            fileCard.Controls.Add(files);
            root.Controls.Add(fileCard, 0, 3);

            TableLayoutPanel footer = new TableLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.Margin = new Padding(0, 10, 0, 0);
            footer.ColumnCount = 3;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            extract.Text = "Extract selected";
            extract.Click += ExtractClick;
            StyleButton(extract, true);
            status.Dock = DockStyle.Fill;
            status.ForeColor = MutedColor;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Margin = new Padding(14, 0, 0, 0);
            status.Text = "Select an APFS partition image or enter a device path such as \\\\.\\PhysicalDrive1.";
            footer.Controls.Add(extract, 0, 0);
            footer.Controls.Add(status, 1, 0);
            footer.Controls.Add(BuildFooterLinks(), 2, 0);
            root.Controls.Add(footer, 0, 4);
            Controls.Add(root);
            Shown += delegate { ResizeFileColumns(); };
        }

        /// <summary>
        /// The freeware notice and the author and donation links, always visible so the tool can be
        /// credited and supported without opening the About window.
        /// </summary>
        private static FlowLayoutPanel BuildFooterLinks()
        {
            FlowLayoutPanel links = new FlowLayoutPanel();
            links.AutoSize = true;
            links.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            links.Anchor = AnchorStyles.Right;
            links.BackColor = Color.Transparent;
            links.FlowDirection = FlowDirection.LeftToRight;
            links.WrapContents = false;
            links.Margin = new Padding(14, 8, 0, 0);

            Label freeware = CreateLabel("Freeware", 8.5F, FontStyle.Regular, MutedColor);
            freeware.AutoSize = true;
            freeware.Margin = new Padding(0, 3, 0, 0);
            links.Controls.Add(freeware);
            links.Controls.Add(CreateSeparator());
            links.Controls.Add(AboutDialog.NewLink("Patrick Hamid", BuildInfo.AuthorUrl,
                "Open linkedin.com/in/phamid in your browser"));
            links.Controls.Add(CreateSeparator());
            links.Controls.Add(AboutDialog.NewLink("\u2615 Buy me a coffee", BuildInfo.DonationUrl,
                "Support this free tool - opens " + BuildInfo.DonationUrl));
            return links;
        }

        private static Label CreateSeparator()
        {
            Label separator = CreateLabel("\u00B7", 9F, FontStyle.Regular, BorderColor);
            separator.AutoSize = true;
            separator.Margin = new Padding(6, 3, 6, 0);
            return separator;
        }

        private void ShowAbout()
        {
            using (AboutDialog dialog = new AboutDialog())
                dialog.ShowDialog(this);
        }

        private void ResizeFileColumns()
        {
            if (files.Columns.Count != 3)
                return;
            files.Columns[0].Width = Math.Max(300, files.ClientSize.Width - 260);
            files.Columns[1].Width = 120;
            files.Columns[2].Width = 140;
        }

        private static Panel CreateCard()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(16, 10, 16, 10);
            panel.BackColor = SurfaceColor;
            panel.Paint += delegate(object sender, PaintEventArgs e) {
                Control control = (Control)sender;
                using (Pen pen = new Pen(BorderColor))
                    e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
            };
            return panel;
        }

        private static Label CreateLabel(string text, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", size, style);
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            return label;
        }

        private static Label CreateFieldLabel(string text)
        {
            Label label = CreateLabel(text, 8F, FontStyle.Bold, MutedColor);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private static void StyleInput(TextBox input)
        {
            input.BackColor = ElevatedColor;
            input.ForeColor = TextColor;
            input.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void StyleButton(Button button, bool primary)
        {
            button.AutoSize = false;
            button.Size = new Size(primary ? 112 : 88, 32);
            button.Margin = new Padding(8, 4, 0, 4);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = primary ? AccentColor : BorderColor;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(89, 229, 243) : SurfaceSoftColor;
            button.FlatAppearance.MouseDownBackColor = AccentHoverColor;
            button.BackColor = primary ? AccentColor : ElevatedColor;
            button.ForeColor = primary ? BackgroundColor : TextColor;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.UseVisualStyleBackColor = false;
        }

        private static void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush background = new SolidBrush(ElevatedColor))
                e.Graphics.FillRectangle(background, e.Bounds);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= e.Header.TextAlign == HorizontalAlignment.Right
                ? TextFormatFlags.Right : TextFormatFlags.Left;
            Rectangle textBounds = new Rectangle(e.Bounds.X + 10, e.Bounds.Y,
                e.Bounds.Width - 20, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, new Font("Segoe UI", 8F, FontStyle.Bold),
                textBounds, MutedColor, flags);
            using (Pen pen = new Pen(BorderColor))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1,
                    e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private static void DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item.Selected;
            Color rowColor = e.ItemIndex % 2 == 0 ? SurfaceColor : Color.FromArgb(17, 24, 57);
            using (SolidBrush background = new SolidBrush(selected ? SurfaceSoftColor : rowColor))
                e.Graphics.FillRectangle(background, e.Bounds);

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= e.Header.TextAlign == HorizontalAlignment.Right
                ? TextFormatFlags.Right : TextFormatFlags.Left;
            Rectangle textBounds = new Rectangle(e.Bounds.X + 10, e.Bounds.Y,
                e.Bounds.Width - 20, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, textBounds,
                selected ? AccentColor : TextColor, flags);
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select an APFS partition image";
                dialog.Filter = "Disk images (*.img;*.dmg;*.apfs)|*.img;*.dmg;*.apfs|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    source.Text = dialog.FileName;
            }
        }

        private void LoadFolder(string requestedPath)
        {
            if (String.IsNullOrWhiteSpace(source.Text))
            {
                MessageBox.Show(this, "Select an APFS source first.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true, "Reading...");
            string selectedSource = source.Text;
            string selectedPassword = password.Text;
            int selectedVolume = Decimal.ToInt32(volume.Value);
            Task.Factory.StartNew(delegate {
                return backend.List(selectedSource, requestedPath, selectedPassword, selectedVolume);
            }).ContinueWith(delegate(Task<IList<ApfsEntry>> task) {
                SetBusy(false, String.Empty);
                if (task.IsFaulted)
                {
                    ShowError(task.Exception.GetBaseException());
                    return;
                }
                path = requestedPath;
                currentPath.Text = path;
                files.Items.Clear();
                foreach (ApfsEntry entry in task.Result)
                {
                    ListViewItem item = new ListViewItem(entry.Name);
                    item.SubItems.Add(entry.IsDirectory ? "Folder" : entry.Type == 'L' ? "Link" : "File");
                    item.SubItems.Add(entry.IsDirectory ? String.Empty : FormatSize(entry.Size));
                    item.Tag = entry;
                    files.Items.Add(item);
                }
                status.Text = task.Result.Count.ToString(CultureInfo.CurrentCulture) + " item(s)";
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void FilesDoubleClick(object sender, EventArgs e)
        {
            if (files.SelectedItems.Count != 1)
                return;
            ApfsEntry entry = (ApfsEntry)files.SelectedItems[0].Tag;
            if (entry.IsDirectory)
                LoadFolder(CombineApfs(path, entry.Name));
        }

        private void ExtractClick(object sender, EventArgs e)
        {
            if (files.SelectedItems.Count == 0)
                return;

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose where to extract the selected APFS items";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                List<ApfsEntry> selected = new List<ApfsEntry>();
                foreach (ListViewItem item in files.SelectedItems)
                    selected.Add((ApfsEntry)item.Tag);

                SetBusy(true, "Extracting...");
                string selectedSource = source.Text;
                string selectedPassword = password.Text;
                int selectedVolume = Decimal.ToInt32(volume.Value);
                string destination = dialog.SelectedPath;
                Task.Factory.StartNew(delegate {
                    foreach (ApfsEntry entry in selected)
                        ExtractEntry(selectedSource, selectedPassword, selectedVolume,
                            CombineApfs(path, entry.Name), Path.Combine(destination, SafeName(entry.Name)), entry);
                }).ContinueWith(delegate(Task task) {
                    SetBusy(false, String.Empty);
                    if (task.IsFaulted)
                        ShowError(task.Exception.GetBaseException());
                    else
                        status.Text = "Extraction complete.";
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void ExtractEntry(string selectedSource, string selectedPassword, int selectedVolume,
            string apfsPath, string destination, ApfsEntry entry)
        {
            if (!entry.IsDirectory)
            {
                backend.Extract(selectedSource, apfsPath, destination, selectedPassword, selectedVolume);
                return;
            }

            Directory.CreateDirectory(destination);
            foreach (ApfsEntry child in backend.List(selectedSource, apfsPath, selectedPassword, selectedVolume))
                ExtractEntry(selectedSource, selectedPassword, selectedVolume,
                    CombineApfs(apfsPath, child.Name), Path.Combine(destination, SafeName(child.Name)), child);
        }

        private void SetBusy(bool busy, string message)
        {
            browse.Enabled = open.Enabled = up.Enabled = extract.Enabled = !busy;
            UseWaitCursor = busy;
            if (message.Length != 0)
                status.Text = message;
        }

        private void ShowError(Exception error)
        {
            status.Text = "Operation failed.";
            MessageBox.Show(this, error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static string CombineApfs(string parent, string name)
        {
            return parent == "/" ? "/" + name : parent.TrimEnd('/') + "/" + name;
        }

        private static string ParentPath(string value)
        {
            if (value == "/")
                return "/";
            int slash = value.LastIndexOf('/');
            return slash <= 0 ? "/" : value.Substring(0, slash);
        }

        private static string SafeName(string name)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return name == "." || name == ".." || name.Length == 0 ? "_" : name;
        }

        private static string FormatSize(long size)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = size;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            return value.ToString(unit == 0 ? "0" : "0.0", CultureInfo.CurrentCulture) + " " + units[unit];
        }
    }
}
