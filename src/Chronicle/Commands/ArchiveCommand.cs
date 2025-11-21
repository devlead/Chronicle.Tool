using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chronicle.Commands.Settings;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Chronicle.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArchiveCommand : AsyncCommand<ArchiveSettings>
    {
        private ILogger Logger { get; }
        public ArchiveCommand(ILogger<ArchiveCommand> logger)
        {
            Logger = logger;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ArchiveSettings settings, CancellationToken cancellationToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var startDate = DateTime.Now;
            long archivedFileCount = 0,
                deletedFileCount = 0,
                processedFileCount = 0;
            try
            {
              
                
                var minDate = settings.GetMinDate(startDate);
                Logger.LogInformation(
                    "Will be archiving files older than {minDate:yyyy-MM-dd HH:mm} from {SourceDirectory} to {TargetDirectory} with file pattern {FilePattern} and target zip prefix {ArchivePrefix}",
                    minDate,
                    settings.SourceDirectory,
                    settings.TargetDirectory,
                    settings.FilePattern,
                    settings.ArchivePrefix
                );

                foreach (var fileGroup in Directory
                    .EnumerateFiles(settings.SourceDirectory, settings.FilePattern)
                    .Select(filePath => (filePath, fileDate: File.GetLastWriteTime(filePath)))
                    .Where(file => file.fileDate < minDate)
                    .GroupBy(
                        file => (file.fileDate.Year, file.fileDate.Month, file.fileDate.Day, file.fileDate.Hour),
                        file => file
                    )
                )
                {
                    var targetArchive = Path.Combine(
                        settings.TargetDirectory,
                        $"{settings.ArchivePrefix}_{fileGroup.Key.Year:0000}{fileGroup.Key.Month:00}{fileGroup.Key.Day:00}{fileGroup.Key.Hour:00}.zip"
                    );

                    Logger.LogInformation("Creating zip file {targetArchive}...", targetArchive);

                    await using var outputStream = File.Open(targetArchive, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                        FileShare.None);
                    using var archive = new ZipArchive(outputStream, ZipArchiveMode.Update);

                    foreach (var (filePath, fileDate) in fileGroup)
                    {
                        if (++processedFileCount % settings.DisplayProgressAfter == 0)
                        {
                            Logger.LogInformation(
                                "Currently archived files {archivedFileCount}, deleted {deletedFileCount}, total processed {processedFileCount} in {Elapsed}",
                                archivedFileCount,
                                deletedFileCount,
                                processedFileCount,
                                stopwatch.Elapsed
                            );
                        }

                        var entry = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Optimal);
                        entry.LastWriteTime = GetValidZipDateTimeOffset(fileDate);
                        await using (Stream fileStream = File.OpenRead(filePath),
                            entryStream = entry.Open())
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }

                        archivedFileCount++;
                    }

                    Logger.LogInformation("Zip file {targetArchive} created.", targetArchive);

                    Logger.LogInformation("Deleting files added to zip file {targetArchive}...", targetArchive);
                    foreach (var (filePath, _) in fileGroup)
                    {
                        File.Delete(filePath);
                        deletedFileCount++;
                    }
                    Logger.LogInformation("Deleted files added to zip file {targetArchive}.", targetArchive);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error during archive");
                return 1;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogInformation(
                    "Archived files {archivedFileCount}, deleted {deletedFileCount}, total processed {processedFileCount} in {Elapsed}",
                    archivedFileCount,
                    deletedFileCount,
                    processedFileCount,
                    stopwatch.Elapsed
                    );
            }

            Logger.LogInformation("Done");
            return 0;
        }

        private static DateTimeOffset GetValidZipDateTimeOffset(DateTime? value)
        {
            var offsetValue = value ?? DateTime.UtcNow;
            if (offsetValue.Year >= ValidZipDateYearMin && offsetValue.Year <= ValidZipDateYearMax)
            {
                return offsetValue;
            }

            return InvalidZipDateIndicator;
        }

        private const int ValidZipDateYearMin = 1980;
        private const int ValidZipDateYearMax = 2107;
        private static readonly DateTimeOffset InvalidZipDateIndicator = new (ValidZipDateYearMin, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
