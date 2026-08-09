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
        private readonly TextBox source = new TextBox();
        private readonly TextBox password = new TextBox();
        private readonly NumericUpDown volume = new NumericUpDown();
        private readonly TextBox currentPath = new TextBox();
        private readonly ListView files = new ListView();
        private readonly Button browse = new Button();
        private readonly Button open = new Button();
        private readonly Button up = new Button();
        private readonly Button extract = new Button();
        private readonly Label status = new Label();
        private readonly ApfsBackend backend = new ApfsBackend();
        private string path = "/";

        public MainForm()
        {
            Text = "APFS Reader for Windows";
            MinimumSize = new Size(760, 480);
            Size = new Size(940, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BuildLayout();
        }

        private void BuildLayout()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(10);
            root.ColumnCount = 4;
            root.RowCount = 5;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            source.Dock = DockStyle.Fill;
            browse.Text = "Browse...";
            browse.AutoSize = true;
            browse.Click += BrowseClick;
            open.Text = "Open";
            open.AutoSize = true;
            open.Click += delegate { LoadFolder("/"); };
            root.Controls.Add(new Label { Text = "APFS source:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            root.Controls.Add(source, 1, 0);
            root.Controls.Add(browse, 2, 0);
            root.Controls.Add(open, 3, 0);

            password.UseSystemPasswordChar = true;
            password.Dock = DockStyle.Fill;
            volume.Minimum = 1;
            volume.Maximum = 100;
            volume.Value = 1;
            volume.Width = 55;
            root.Controls.Add(new Label { Text = "Password:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            root.Controls.Add(password, 1, 1);
            root.Controls.Add(new Label { Text = "Volume:", AutoSize = true, Anchor = AnchorStyles.Right }, 2, 1);
            root.Controls.Add(volume, 3, 1);

            FlowLayoutPanel navigation = new FlowLayoutPanel();
            navigation.AutoSize = true;
            navigation.Dock = DockStyle.Fill;
            up.Text = "Up";
            up.AutoSize = true;
            up.Click += delegate { LoadFolder(ParentPath(path)); };
            currentPath.ReadOnly = true;
            currentPath.Width = 560;
            currentPath.Text = "/";
            navigation.Controls.Add(up);
            navigation.Controls.Add(currentPath);
            root.Controls.Add(navigation, 0, 2);
            root.SetColumnSpan(navigation, 4);

            files.Dock = DockStyle.Fill;
            files.View = View.Details;
            files.FullRowSelect = true;
            files.MultiSelect = true;
            files.Columns.Add("Name", 500);
            files.Columns.Add("Type", 100);
            files.Columns.Add("Size", 130, HorizontalAlignment.Right);
            files.DoubleClick += FilesDoubleClick;
            root.Controls.Add(files, 0, 3);
            root.SetColumnSpan(files, 4);

            FlowLayoutPanel footer = new FlowLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.AutoSize = true;
            extract.Text = "Extract selected...";
            extract.AutoSize = true;
            extract.Click += ExtractClick;
            status.AutoSize = true;
            status.Margin = new Padding(12, 7, 0, 0);
            status.Text = "Select an APFS partition image or enter a device path such as \\\\.\\PhysicalDrive1.";
            footer.Controls.Add(extract);
            footer.Controls.Add(status);
            root.Controls.Add(footer, 0, 4);
            root.SetColumnSpan(footer, 4);
            Controls.Add(root);
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
