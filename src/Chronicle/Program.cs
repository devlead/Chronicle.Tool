using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Chronicle.Commands;
using Spectre.Cli;
using Spectre.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection()
    .AddLogging(configure =>
        configure.AddSimpleConsole(opts => { opts.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; }));

using var registrar = new DependencyInjectionRegistrar(serviceCollection);

var app = new CommandApp(registrar);

app.Configure(config => {
    config.AddCommand<ArchiveCommand>("archive")
        .WithExample(new[]
        {
            "archive", "\"C:\\temp\\Chronicle\\source\"", "\"C:\\temp\\Chronicle\\target\" \"*.log\"", "\"LogFiles\""
        });
    config.AddCommand<MoveCommand>("move")
        .WithExample(new[]
        {
            "move", "\"C:\\temp\\Chronicle\\source\"", "\"C:\\temp\\Chronicle\\target\" \"*.log\""
        });
});

return await app.RunAsync(args);