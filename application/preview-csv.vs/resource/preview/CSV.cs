
using Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace resource.preview
{
    public class CSV : cartridge.AnyPreview
    {
        protected override void _Execute(atom.Trace context, string url)
        {
            var a_Context = new CsvOptions
            {
                RowsToSkip = 0, // Allows skipping of initial rows without csv data
                SkipRow = (row, idx) => string.IsNullOrEmpty(row) || row[0] == '#',
                Separator = '\0', // Autodetects based on first row
                TrimData = true, // Can be used to trim each cell
                Comparer = null, // Can be used for case-insensitive comparison for names
                HeaderMode = HeaderMode.HeaderAbsent, // Assumes first row is a header row
                ValidateColumnCount = false, // Checks each row immediately for column count
                ReturnEmptyForMissingColumn = false, // Allows for accessing invalid column names
                Aliases = null, // A collection of alternative column names
                AllowNewLineInEnclosedFieldValues = false, // Respects new line (either \r\n or \n) characters inside field values enclosed in double quotes.
                AllowBackSlashToEscapeQuote = true, // Allows the sequence "\"" to be a valid quoted value (in addition to the standard """")
                AllowSingleQuoteToEncloseFieldValues = true, // Allows the single-quote character to be used to enclose field values
                NewLine = Environment.NewLine // The new line string to use when multiline field values are read (Requires "AllowNewLineInEnclosedFieldValues" to be set to "true" for this to have any effect.)
            };
            {
                var a_Context1 = CsvReader.ReadFromText(File.ReadAllText(url), a_Context);
                if (a_Context1 != null)
                {
                    __Execute(a_Context1, 1, context, url);
                }
            }
        }

        private static void __Execute(IEnumerable<ICsvLine> node, int level, atom.Trace context, string url)
        {
            var a_Index = 0;
            foreach (var a_Context in node)
            {
                {
                    a_Index++;
                }
                {
                    __Execute(a_Context, level, context, a_Index, url);
                }
            }
        }

        private static void __Execute(ICsvLine node, int level, atom.Trace context, int index, string url)
        {
            if ((index == 1) && __IsCaption(node))
            {
                context.
                    SetComment("<[[Caption]]>").
                    SetHint("<[[Row type]]>").
                    SetFlag(NAME.FLAG.HIGHLIGHT);
            }
            else
            {
                context.
                    SetComment("[" + index.ToString("D4") + "]").
                    SetHint("[[[Row number]]]"); 
            }
            {
                context.
                    SetUrl(url).
                    SetLine(node.Index).
                    Send(NAME.PATTERN.PREVIEW, level, "ROW");
            }
            foreach (var a_Context in node.Values)
            {
                context.
                    Send(NAME.PATTERN.ELEMENT, level + 1, __GetName(a_Context));
            }
        }

        private static bool __IsCaption(ICsvLine node)
        {
            var a_Result = true;
            foreach (var a_Context in node.Values)
            {
                var a_Context1 = __GetName(a_Context);
                var a_Context2 = 0.0;
                if (string.IsNullOrEmpty(a_Context1) || double.TryParse(a_Context1, NumberStyles.Any, CultureInfo.InvariantCulture, out a_Context2))
                {
                    return false;
                }
            }
            return a_Result;
        }

        private static string __GetName(string value)
        {
            var a_Result = GetCleanString(value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            if (a_Result.Length >= 2)
            {
                var a_Index = a_Result.Length - 1;
                if ((a_Result[0] == '\"') && (a_Result[a_Index] == '\"'))
                {
                    a_Result = a_Result.Substring(1, a_Index - 1);
                }
                if ((a_Result[0] == '\'') && (a_Result[a_Index] == '\''))
                {
                    a_Result = a_Result.Substring(1, a_Index - 1);
                }
            }
            return a_Result.Trim();
        }
    };
}