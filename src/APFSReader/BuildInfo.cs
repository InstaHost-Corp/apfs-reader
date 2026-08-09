using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace APFSReader
{
    /// <summary>
    /// Everything the About window reports about the copy of the tool that is actually running:
    /// version, build date, publisher and the SHA-256 of the file on disk, so provenance can be
    /// checked without a network connection.
    /// </summary>
    internal static class BuildInfo
    {
        public const string Author = "Patrick Hamid";
        public const string Licence = "Freeware - free to use and share, no charge, no licence key.";

        public const string ToolsUrl = "https://insta.host/tools";
        public const string PrivacyUrl = "https://insta.host/privacy";
        public const string DonationUrl = "https://coffee.insta.host";
        public const string AuthorUrl = "https://linkedin.com/in/phamid";

        private static Assembly Self
        {
            get { return typeof(BuildInfo).Assembly; }
        }

        public static string Product
        {
            get
            {
                AssemblyProductAttribute product = GetAttribute<AssemblyProductAttribute>();
                return product == null || string.IsNullOrEmpty(product.Product)
                    ? "APFS Reader for Windows"
                    : product.Product;
            }
        }

        public static string Publisher
        {
            get
            {
                AssemblyCompanyAttribute company = GetAttribute<AssemblyCompanyAttribute>();
                return company == null || string.IsNullOrEmpty(company.Company)
                    ? "MyTechie"
                    : company.Company;
            }
        }

        public static string Copyright
        {
            get
            {
                AssemblyCopyrightAttribute copyright = GetAttribute<AssemblyCopyrightAttribute>();
                return copyright == null ? string.Empty : copyright.Copyright;
            }
        }

        /// <summary>The three-part product version, e.g. "1.2.0".</summary>
        public static string Version
        {
            get
            {
                AssemblyInformationalVersionAttribute informational =
                    GetAttribute<AssemblyInformationalVersionAttribute>();
                if (informational != null && !string.IsNullOrEmpty(informational.InformationalVersion))
                    return informational.InformationalVersion.Split('+')[0];

                Version version = Self.GetName().Version;
                return version == null ? "0.0.0" : version.ToString(3);
            }
        }

        /// <summary>The running executable, or null when it cannot be determined.</summary>
        public static string ExecutablePath
        {
            get
            {
                try
                {
                    string path = Assembly.GetEntryAssembly() == null
                        ? Self.Location
                        : Assembly.GetEntryAssembly().Location;
                    return string.IsNullOrEmpty(path) || !File.Exists(path) ? null : path;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>The UTC timestamp of the running executable, or "unknown".</summary>
        public static string BuildDate
        {
            get
            {
                string path = ExecutablePath;
                if (path == null)
                    return "unknown";

                try
                {
                    return File.GetLastWriteTimeUtc(path).ToString("yyyy-MM-dd HH:mm 'UTC'");
                }
                catch
                {
                    return "unknown";
                }
            }
        }

        /// <summary>
        /// SHA-256 of the running executable, so it can be compared against the checksum published
        /// on the download page. Returns null when the file cannot be read.
        /// </summary>
        public static string FileSha256()
        {
            string path = ExecutablePath;
            if (path == null)
                return null;

            try
            {
                using (FileStream stream = File.OpenRead(path))
                using (SHA256 sha = new SHA256Managed())
                    return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }

        private static T GetAttribute<T>() where T : Attribute
        {
            object[] attributes = Self.GetCustomAttributes(typeof(T), false);
            return attributes.Length == 0 ? null : (T)attributes[0];
        }
    }
}
