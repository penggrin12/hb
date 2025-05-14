using System;
using System.IO;
using CommandLine;

namespace HighBasic;

public static class Program
{
    public class Options
    {
        [Option("debug", Required = false, Default = false, HelpText = "Enable debug mode")]
        public bool Debug { get; set; }

        [Value(0, Required = true, MetaName = "INPUT", HelpText = "The code file to run")]
        public string InputFile { get; set; } = string.Empty;
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                using FileStream inputFile = File.Open(o.InputFile, FileMode.Open);
                using StreamReader reader = new(inputFile);

                Runtime runtime = new Runtime()
                    .InsertStandardLibrary();

                if (o.Debug)
                    runtime.Debug = true;

                runtime.DoString(reader.ReadToEnd());
            });
    }
}
