using System.Drawing;

namespace APFSReader
{
    /// <summary>The InstaHost desktop palette shared by every window in the application.</summary>
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(4, 6, 15);
        public static readonly Color Elevated = Color.FromArgb(10, 16, 48);
        public static readonly Color Surface = Color.FromArgb(20, 28, 64);
        public static readonly Color SurfaceSoft = Color.FromArgb(25, 35, 76);
        public static readonly Color Border = Color.FromArgb(47, 64, 112);
        public static readonly Color Text = Color.FromArgb(234, 240, 255);
        public static readonly Color Muted = Color.FromArgb(147, 160, 207);
        public static readonly Color Accent = Color.FromArgb(34, 211, 238);
        public static readonly Color AccentHover = Color.FromArgb(168, 85, 247);
    }
}
