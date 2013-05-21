using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Options;

using Hanasaku;

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("cel2tiff.cs")]
[assembly: AssemblyDescription("Converts .cel to a multipage .tiff.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Nekozakura")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("2011-*")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.0.0.0")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]



public class Entrypoint
{
    static void ShowHelp(OptionSet p)
    {
        ShowHelp(p, String.Empty);
    }
    static void ShowHelp(OptionSet p, string triggeringMessage)
    {
        if (triggeringMessage != String.Empty)
        {
            System.Console.Error.WriteLine("Error: " + triggeringMessage);
        }
        System.Console.Out.WriteLine("cel2tiff v1.0, Copyright (C) 2011 Nanashi3" + Environment.NewLine
        + "  Portions, Copyright (C) 2008 Novell, (C) 2009 Federico Di Gregorio." + Environment.NewLine
        + "  This product includes software developed by Jon Skeet and Marc Gravell." + Environment.NewLine
        + "  Contact skeet@pobox.com, or see http://www.pobox.com/~skeet/)." + Environment.NewLine);
        System.Console.Out.WriteLine("ConvertCelToTiff a SUCCESS .cel image container into a .tiff image");
        System.Console.Out.WriteLine("Usage: cel2tiff <input> [<output>] [--desc <outpath>]" + Environment.NewLine);
        System.Console.Out.WriteLine("Options:");
        p.WriteOptionDescriptions(System.Console.Out);
    }

    static OptionSet p;


    static void Main(string[] args)
    {
        List<string> pathlist;
        string inputpath = "", outputpath = "", descpath = "";

        p = new OptionSet()
          .Add("desc=", "(INFORMATIVE) File path to write .cel descriptive data into.", v => descpath = v)
          .Add("?|h|help", "Show this help screen.", v => { ShowHelp(p); Environment.Exit(0); });

        try
        {
            pathlist = p.Parse(args);
            if (pathlist.Count == 0)
            {
                throw new OptionException("Missing input path.", "input");
            }
            inputpath = pathlist[0];
            pathlist.RemoveAt(0);
            if (!File.Exists(inputpath))
            {
                throw new OptionException(String.Format("Input path \"{0}\" does not exist or is not a regular file.", inputpath), "input");
            }
            outputpath = (pathlist.Count > 0) ? pathlist[0] : inputpath + ".tiff";
            if (Path.GetFullPath(inputpath) == Path.GetFullPath(outputpath))
            {
                throw new OptionException(String.Format("Output path \"{0}\" should differ from input path.", outputpath), "output");
            }
            if (Directory.Exists(outputpath))
            {
                throw new OptionException(String.Format("Output path \"{0}\" must be a file.", outputpath), "output");
            }
        }
        catch (OptionException ex)
        {
            ShowHelp(p, ex.Message);
            Environment.Exit(1);
        }

        // Assume something went wrong
        int exitCode = 2;

        try
        {
            using (FileStream ifs = File.OpenRead(inputpath))
            using (FileStream ofs = File.OpenWrite(outputpath))
            {
                ofs.SetLength(0);
                Hanasaku.cel2tiff.ConvertCelToTiff(ifs, ofs, Path.ChangeExtension(outputpath, null));
            }
            exitCode = 0;
        }
        catch (ApplicationException ex)
        {
            System.Console.Error.WriteLine("Error: " + ex.Message);
        }
#if !DEBUG
        catch (Exception e)
        {
            // We don't really expect non-ApplicationException's - generate an error-report if we see one
            System.Console.Error.WriteLine("Unhandled Exception: " + e.Message);
            System.Console.Error.WriteLine("Stack Trace: " + Environment.NewLine + e.StackTrace);
        }
#endif
        finally
        {
            Environment.Exit(exitCode);
        }

    }
}
