using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace APFSReader
{
    /// <summary>
    /// Provenance a technician can check without a network: version, build date, publisher and the
    /// SHA-256 of the running file, plus the freeware statement and author links.
    /// </summary>
    internal sealed class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About APFS Reader";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(486, 388);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = new Font("Segoe UI", 9F);

            Label eyebrow = NewLabel("INSTA.HOST TOOLING", 8F, FontStyle.Bold, Theme.Accent);
            eyebrow.Location = new Point(20, 20);

            Label title = NewLabel(BuildInfo.Product, 14F, FontStyle.Bold, Theme.Text);
            title.Location = new Point(19, 40);

            Label version = NewLabel(
                "Version " + BuildInfo.Version + "   \u00B7   built " + BuildInfo.BuildDate,
                9F, FontStyle.Regular, Theme.Muted);
            version.Location = new Point(20, 70);

            Label licence = NewLabel(BuildInfo.Licence, 9F, FontStyle.Regular, Theme.Text);
            licence.Location = new Point(20, 98);

            Label publisher = NewLabel(
                "Publisher: " + BuildInfo.Publisher + "\r\nAuthor: " + BuildInfo.Author + "\r\n" + BuildInfo.Copyright,
                9F, FontStyle.Regular, Theme.Text);
            publisher.AutoSize = false;
            publisher.Location = new Point(20, 124);
            publisher.Size = new Size(446, 54);

            Label checksumLabel = NewLabel(
                "SHA-256 of this file - compare it with the download page:",
                8.5F, FontStyle.Regular, Theme.Muted);
            checksumLabel.Location = new Point(20, 192);

            TextBox checksum = new TextBox();
            checksum.Text = BuildInfo.FileSha256() ?? "unavailable";
            checksum.Location = new Point(20, 212);
            checksum.Size = new Size(446, 40);
            checksum.ReadOnly = true;
            checksum.Multiline = true;
            checksum.BorderStyle = BorderStyle.FixedSingle;
            checksum.BackColor = Theme.Elevated;
            checksum.ForeColor = Theme.Text;
            checksum.Font = new Font("Consolas", 8.5F);

            Label path = NewLabel(BuildInfo.ExecutablePath ?? string.Empty, 8.5F, FontStyle.Regular, Theme.Muted);
            path.AutoSize = false;
            path.AutoEllipsis = true;
            path.Location = new Point(20, 262);
            path.Size = new Size(446, 18);

            FlowLayoutPanel links = new FlowLayoutPanel();
            links.Location = new Point(16, 292);
            links.Size = new Size(454, 30);
            links.BackColor = Color.Transparent;
            links.FlowDirection = FlowDirection.LeftToRight;
            links.WrapContents = false;
            links.Controls.Add(NewLink("Patrick Hamid", BuildInfo.AuthorUrl, "Open linkedin.com/in/phamid in your browser"));
            links.Controls.Add(NewSeparator());
            links.Controls.Add(NewLink("More free tools", BuildInfo.ToolsUrl, "Opens " + BuildInfo.ToolsUrl));
            links.Controls.Add(NewSeparator());
            links.Controls.Add(NewLink("Privacy", BuildInfo.PrivacyUrl, "Opens " + BuildInfo.PrivacyUrl));
            links.Controls.Add(NewSeparator());
            links.Controls.Add(NewLink("\u2615 Buy me a coffee", BuildInfo.DonationUrl, "Support this free tool - opens " + BuildInfo.DonationUrl));

            Button close = new Button();
            close.Text = "Close";
            close.DialogResult = DialogResult.OK;
            close.Bounds = new Rectangle(374, 336, 92, 32);
            close.FlatStyle = FlatStyle.Flat;
            close.FlatAppearance.BorderSize = 1;
            close.FlatAppearance.BorderColor = Theme.Accent;
            close.FlatAppearance.MouseOverBackColor = Color.FromArgb(89, 229, 243);
            close.FlatAppearance.MouseDownBackColor = Theme.AccentHover;
            close.BackColor = Theme.Accent;
            close.ForeColor = Theme.Background;
            close.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            close.UseVisualStyleBackColor = false;

            Controls.AddRange(new Control[]
            {
                eyebrow, title, version, licence, publisher,
                checksumLabel, checksum, path, links, close
            });
            AcceptButton = close;
            CancelButton = close;

            Theme.Apply(this);
        }

        private static Label NewLabel(string text, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", size, style);
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            label.AutoSize = true;
            return label;
        }

        private static Label NewSeparator()
        {
            Label separator = NewLabel("\u00B7", 9F, FontStyle.Regular, Theme.Border);
            separator.Margin = new Padding(6, 3, 6, 0);
            return separator;
        }

        internal static LinkLabel NewLink(string text, string url, string tooltip)
        {
            LinkLabel link = new LinkLabel();
            link.Text = text;
            link.AutoSize = true;
            link.BackColor = Color.Transparent;
            link.LinkColor = Theme.Accent;
            link.ActiveLinkColor = Theme.AccentHover;
            link.VisitedLinkColor = Theme.Accent;
            link.LinkBehavior = LinkBehavior.HoverUnderline;
            link.Margin = new Padding(0, 3, 0, 0);

            new ToolTip().SetToolTip(link, tooltip);
            link.LinkClicked += delegate { OpenUrl(url); };
            return link;
        }

        internal static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open " + url + "\r\n\r\n" + ex.Message, "APFS Reader",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
