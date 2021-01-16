using System;
using System.ComponentModel;
using System.IO;
using Chronicle.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Chronicle.Commands.Settings
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MoveSettings : CommandSettings
    {
        [CommandArgument(0, "<source directory>")]
        [Description("Source directory path")]
        public string SourceDirectory { get; set; }

        [CommandArgument(1, "<target directory>")]
        [Description("Target directory path")]
        public string TargetDirectory { get; set; }

        [CommandArgument(2, "<pattern>")]
        [Description("Source file pattern")]
        public string FilePattern { get; set; }

        [CommandOption("--min-age")]
        [Description("Minimum age of file 1 day")]
        public uint MinAge { get; set; } = 1;

        [CommandOption("--age-unit")]
        [Description("Age unit day (default), hour, minute")]
        public AgeUnit AgeUnit { get; set; } = AgeUnit.Day;

        [CommandOption("--display-progress-after")]
        [Description("Display progress after specified number of files processed, default 5000")]
        public long DisplayProgressAfter { get; set; } = 5000;

        public DateTime GetMinDate(DateTime date) => AgeUnit switch
        {
            AgeUnit.Minute => date.AddMinutes(-MinAge),
            AgeUnit.Hour => date.AddHours(-MinAge),
            AgeUnit.Day => date.AddDays(-MinAge),
            _ => throw new ArgumentOutOfRangeException("AgeUnit", AgeUnit, "Unsupported AgeUnit")
        };

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(SourceDirectory) || !Directory.Exists(SourceDirectory))
            {
                return ValidationResult.Error($"Missing source or invalid directory: {SourceDirectory}");
            }

            if (string.IsNullOrWhiteSpace(TargetDirectory) || !Directory.Exists(TargetDirectory))
            {
                return ValidationResult.Error($"Missing target or invalid directory: {TargetDirectory}");
            }

            return base.Validate();
        }
    }
}