using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace APFSReader
{
    internal sealed class ApfsEntry
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public char Type { get; set; }
        public bool IsDirectory { get { return Type == 'D'; } }
    }

    internal sealed class ApfsBackend
    {
        private readonly string executable;

        public ApfsBackend()
        {
            executable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apfs_backend.exe");
        }

        public IList<ApfsEntry> List(string source, string path, string password, int volume)
        {
            string output = Run(
                "enum-machine --source=" + Quote(source) + " --path=" + Quote(path) +
                PasswordArgument(password, volume),
                password);
            return ParseEntries(output);
        }

        public void Extract(string source, string path, string destination, string password, int volume)
        {
            if (File.Exists(destination))
                throw new IOException("A file already exists at " + destination);

            string partial = destination + ".apfsreader-partial-" + Guid.NewGuid().ToString("N");
            try
            {
                Run(
                    "export-file --source=" + Quote(source) + " --path=" + Quote(path) +
                    " --output=" + Quote(partial) + PasswordArgument(password, volume),
                    password);
                File.Move(partial, destination);
            }
            catch
            {
                if (File.Exists(partial))
                    File.Delete(partial);
                throw;
            }
        }

        internal static IList<ApfsEntry> ParseEntries(string output)
        {
            List<ApfsEntry> entries = new List<ApfsEntry>();
            using (StringReader reader = new StringReader(output))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    long size;
                    if (fields.Length != 4 || fields[0] != "ENTRY" ||
                        fields[1].Length != 1 ||
                        !long.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out size))
                        continue;

                    entries.Add(new ApfsEntry {
                        Type = fields[1][0],
                        Size = size,
                        Name = DecodeHexUtf8(fields[3])
                    });
                }
            }
            return entries;
        }

        internal static string DecodeHexUtf8(string value)
        {
            if ((value.Length & 1) != 0)
                throw new FormatException("Invalid APFS entry name.");
            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture);
            return new UTF8Encoding(false, true).GetString(bytes);
        }

        private string Run(string arguments, string password)
        {
            if (!File.Exists(executable))
                throw new FileNotFoundException("The APFS backend is missing.", executable);

            ProcessStartInfo start = new ProcessStartInfo(executable, arguments);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = !String.IsNullOrEmpty(password);

            using (Process process = Process.Start(start))
            {
                if (!String.IsNullOrEmpty(password))
                {
                    process.StandardInput.WriteLine(password);
                    process.StandardInput.Close();
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new IOException(FriendlyError(stderr, process.ExitCode));
                return stdout;
            }
        }

        private static string PasswordArgument(string password, int volume)
        {
            return String.IsNullOrEmpty(password)
                ? String.Empty
                : " --password-stdin=" + volume.ToString(CultureInfo.InvariantCulture);
        }

        private static string Quote(string value)
        {
            if (value == null)
                value = String.Empty;
            if (value.IndexOf('"') >= 0)
                throw new ArgumentException("Paths containing quotation marks are not supported.");
            return "\"" + value + "\"";
        }

        private static string FriendlyError(string stderr, int exitCode)
        {
            string message = (stderr ?? String.Empty).Trim();
            if (message.IndexOf("No password", StringComparison.OrdinalIgnoreCase) >= 0)
                return "This APFS volume is encrypted. Enter its password and try again.";
            if (message.IndexOf("Wrong password", StringComparison.OrdinalIgnoreCase) >= 0)
                return "The APFS password is incorrect.";
            if (message.Length == 0)
                message = "The APFS backend stopped with error " + exitCode + ".";
            return message;
        }
    }
}
