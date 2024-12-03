using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<HashDiffCommand>();
return app.Run(args);

internal sealed class HashDiffCommand : Command<HashDiffCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Path to the first CSV file")]
        [CommandArgument(0, "<file1>")]
        public required FileInfo File1 { get; init; }
        
        [Description("Path to the second CSV file")]
        [CommandArgument(1, "<file2>")]
        public required FileInfo File2 { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var table = new Table();
        table.AddColumn("File");
        table.AddColumn("Hash 1");
        table.AddColumn("Hash 2");
        
        // Read both CSV files
        Dictionary<string, string> hashes1 = new();
        Dictionary<string, string> hashes2 = new();
        
        using (var reader = new StreamReader(settings.File1.FullName))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            foreach (Hashes record in csv.GetRecords<Hashes>())
            {
                hashes1[record.File] = record.Hash;
            }
        }
        using (var reader = new StreamReader(settings.File2.FullName))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            foreach (Hashes record in csv.GetRecords<Hashes>())
            {
                hashes2[record.File] = record.Hash;
            }
        }
        
        // Compare the hashes, it is possible that one csv has more files, print N/A for the missing hash
        foreach (var file in hashes1.Keys.Concat(hashes2.Keys).Distinct())
        {
            var hash1 = hashes1.TryGetValue(file, out var h1) ? h1 : "N/A";
            var hash2 = hashes2.TryGetValue(file, out var h2) ? h2 : "N/A";
            if (hash1 == hash2)
            {
                table.AddRow(new Markup($"[green]{file}[/]"), new Markup($"[green]{hash1}[/]"), new Markup($"[green]{hash2}[/]"));
            }
            else if (hash1 == "N/A" || hash2 == "N/A")
            {
                table.AddRow(new Markup($"[yellow]{file}[/]"), new Markup($"[yellow]{hash1}[/]"), new Markup($"[yellow]{hash2}[/]"));
            }
            else
            {
                table.AddRow(new Markup($"[red]{file}[/]"), new Markup($"[red]{hash1}[/]"), new Markup($"[red]{hash2}[/]"));
            }
        }

        AnsiConsole.Write(table);
        
        return 0;
    }
    
    public sealed class Hashes
    {
        public required string File { get; set; }
        public required string Hash { get; set; }
    }
}