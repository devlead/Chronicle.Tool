using System.ComponentModel;
using System.IO.Compression;
using Spectre.Cli;

namespace Chronicle.Commands.Settings
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArchiveSettings : MoveSettings
    {
       
        [CommandArgument(3, "<archive prefix>")]
        [Description("Target zip file arcive prefix")]
        public string ArchivePrefix { get; set; }



        [CommandOption("--compression-level")]
        [Description(
            "Zip file compression level "
            + nameof(CompressionLevel.NoCompression)
            + ", "
            + nameof(CompressionLevel.Fastest)
            + ", "
            + nameof(CompressionLevel.Optimal)
            + " (default)"
            )]
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
    }
}
