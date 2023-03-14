# Chronicle.Tool

[![NuGet](https://img.shields.io/nuget/v/Chronicle.Tool.svg)](https://www.nuget.org/packages/Chronicle.Tool)

A file archiving .NET Tool

## Installation

```PowerShell
dotnet tool install -g Chronicle.Tool
```

## Usage

```PowerShell
Chronicle [OPTIONS] <COMMAND>
```

### Example Archive to zip

```PowerShell
Chronicle archive "C:\temp\Chronicle\source" "C:\temp\Chronicle\target" "*.log" "LogFiles"
```

### Example Move

```PowerShell
Chronicle move "C:\temp\Chronicle\source" "C:\temp\Chronicle\target" "*.log"
```
