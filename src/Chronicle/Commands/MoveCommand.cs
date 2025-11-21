using System;
using System.IO;
using System.Threading.Tasks;
using Chronicle.Commands.Settings;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Chronicle.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MoveCommand : Command<MoveSettings>
    {
        private ILogger Logger { get; }

        public MoveCommand(ILogger<MoveCommand> logger)
        {
            Logger = logger;
        }

        public override int Execute(CommandContext context, MoveSettings settings, CancellationToken cancellationToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var startDate = DateTime.Now;
            long skippedFileCount = 0,
                movedFileCount = 0,
                processedFileCount = 0,
                errorFileCount = 0;
            try
            {


                var minDate = settings.GetMinDate(startDate);
                Logger.LogInformation(
                    "Will be moving files older than {minDate:yyyy-MM-dd HH:mm} from {SourceDirectory} to {TargetDirectory} with file pattern {FilePattern}",
                    minDate,
                    settings.SourceDirectory,
                    settings.TargetDirectory,
                    settings.FilePattern
                );

                foreach (var filePath in System.IO.Directory.EnumerateFiles(settings.SourceDirectory, settings.FilePattern))
                {
                    if (++processedFileCount % settings.DisplayProgressAfter == 0)
                    {
                        Logger.LogInformation(
                            "Currently moved files {movedFileCount}, skipped {skippedFileCount}, error {errorFileCount}, total processed {processedFileCount} in {Elapsed}",
                            movedFileCount,
                            skippedFileCount,
                            errorFileCount,
                            processedFileCount,
                            stopwatch.Elapsed
                        );
                    }

                    try
                    {
                        var fileDate = File.GetLastWriteTime(filePath);
                        if (fileDate > minDate)
                        {
                            skippedFileCount++;
                            continue;
                        }

                        var targetFilePath = Path.Combine(
                            settings.TargetDirectory,
                            Path.GetFileName(filePath)
                        );

                        File.Move(filePath, targetFilePath, true);

                        movedFileCount++;
                    }
                    catch (Exception)
                    {
                        errorFileCount++;
                    }
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
                    "Moved files {movedFileCount}, skipped {skippedFileCount}, error {errorFileCount}, total processed {processedFileCount} in {Elapsed}",
                    movedFileCount,
                    skippedFileCount,
                    errorFileCount,
                    processedFileCount,
                    stopwatch.Elapsed
                );
            }

            Logger.LogInformation("Done");
            return 0;
        }
    }
}