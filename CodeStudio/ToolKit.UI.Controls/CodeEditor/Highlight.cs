using Avalonia.Media;
using System.Collections.Generic;

namespace Metrino.Development.Studio.Library.Controls
{
    public struct TextFormat
    {
        public string? Foreground = null;

        public FontStyle Style { get; set; } = FontStyle.Normal;
        public FontWeight Weight { get; set; } = FontWeight.Regular;

        public TextFormat()
        {
        }

        public string Key => $"{Foreground}-{Style}-{Weight}";
    }

    public class Highlight
    {
        static Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            ["_nc"] = "#1f1d30",
            ["base"] = "#232136",
            ["surface"] = "#2a273f",
            ["overlay"] = "#393552",
            ["muted"] = "#6e6a86",
            ["subtle"] = "#908caa",
            ["text"] = "#e0def4",
            ["love"] = "#eb6f92",
            ["gold"] = "#f6c177",
            ["rose"] = "#ea9a97",
            ["pine"] = "#3e8fb0",
            ["foam"] = "#9ccfd8",
            ["iris"] = "#c4a7e7",
            ["highlight_low"] = "#2a283e",
            ["highlight_med"] = "#44415a",
            ["highlight_high"] = "#56526e",
            ["none"] = "NONE"
        };


        static public Dictionary<string, TextFormat> CaptureFormats = new Dictionary<string, TextFormat>
        {
            //  Identifiers
            ["variable"] = new TextFormat { Foreground = Palette["text"], Style = FontStyle.Italic },
            ["variable.builtin"] = new TextFormat { Foreground = Palette["love"], Weight = FontWeight.Bold },
            ["variable.parameter"] = new TextFormat { Foreground = Palette["iris"], Style = FontStyle.Italic },
            ["variable.member"] = new TextFormat { Foreground = Palette["foam"] },

            ["constant"] = new TextFormat { Foreground = Palette["gold"] },
            ["constant.builtin"] = new TextFormat { Foreground = Palette["gold"], Weight = FontWeight.Bold },
            ["constant.macro"] = new TextFormat { Foreground = Palette["gold"] },

            ["module"] = new TextFormat { Foreground = Palette["text"] },
            ["module.builtin"] = new TextFormat { Foreground = Palette["text"], Weight = FontWeight.Bold },
            ["label"] = new TextFormat { Foreground = Palette["foam"] },

            // Literals
            ["string"] = new TextFormat { Foreground = Palette["gold"] },
            ["string.documentation"] = new TextFormat { Foreground = Palette["iris"] },
            ["string.regexp"] = new TextFormat { Foreground = Palette["pine"] },
            ["string.escape"] = new TextFormat { Foreground = Palette["gold"] },
            ["string.special"] = new TextFormat { Foreground = Palette["text"] },
            ["string.special.symbol"] = new TextFormat { Foreground = Palette["iris"] },
            ["string.special.url"] = new TextFormat { Foreground = Palette["iris"] },
            ["string.special.path"] = new TextFormat { Foreground = Palette["iris"] },

            ["character"] = new TextFormat { Foreground = Palette["gold"] },
            ["character.special"] = new TextFormat { Foreground = Palette["gold"] },

            ["boolean"] = new TextFormat { Foreground = Palette["rose"] },
            ["number"] = new TextFormat { Foreground = Palette["gold"] },
            ["number.float"] = new TextFormat { Foreground = Palette["gold"] },

            // Types
            ["type"] = new TextFormat { Foreground = Palette["foam"] },
            ["type.builtin"] = new TextFormat { Foreground = Palette["foam"], Weight = FontWeight.Bold },
            ["type.definition"] = new TextFormat { Foreground = Palette["foam"], Weight = FontWeight.Bold },
            ["type.qualifier"] = new TextFormat { Foreground = Palette["foam"], Weight = FontWeight.Bold },

            ["attribute"] = new TextFormat { Foreground = Palette["foam"], Style = FontStyle.Italic },
            ["property"] = new TextFormat { Foreground = Palette["foam"], Style = FontStyle.Italic },

            // Functions
            ["function"] = new TextFormat { Foreground = Palette["rose"] },
            ["function.builtin"] = new TextFormat { Foreground = Palette["rose"], Weight = FontWeight.Bold },
            ["function.call.builtin"] = new TextFormat { Foreground = Palette["rose"], Weight = FontWeight.Bold },
            ["function.call"] = new TextFormat { Foreground = Palette["rose"] },
            ["function.macro"] = new TextFormat { Foreground = Palette["rose"] },
            ["function.method"] = new TextFormat { Foreground = Palette["rose"] },
            ["function.method.call"] = new TextFormat { Foreground = Palette["iris"] },

            ["constructor"] = new TextFormat { Foreground = Palette["foam"] },
            ["operator"] = new TextFormat { Foreground = Palette["subtle"] },

            // Keywords
            ["keyword"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.coroutine"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.function"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.operator"] = new TextFormat { Foreground = Palette["subtle"] },
            ["keyword.import"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.storage"] = new TextFormat { Foreground = Palette["foam"] },
            ["keyword.repeat"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.return"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.debug"] = new TextFormat { Foreground = Palette["rose"] },
            ["keyword.exception"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.conditional"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.conditional.ternary"] = new TextFormat { Foreground = Palette["pine"] },
            ["keyword.directive"] = new TextFormat { Foreground = Palette["iris"] },
            ["keyword.directive.define"] = new TextFormat { Foreground = Palette["iris"] },

            // Punctuation
            ["punctuation.delimiter"] = new TextFormat { Foreground = Palette["subtle"] },
            ["punctuation.bracket"] = new TextFormat { Foreground = Palette["subtle"] },
            ["punctuation.special"] = new TextFormat { Foreground = Palette["subtle"] },

            // Comments
            ["comment"] = new TextFormat { Foreground = Palette["subtle"], Style = FontStyle.Italic },
            ["comment.documentation"] = new TextFormat { Foreground = Palette["subtle"], Style = FontStyle.Italic },

            ["comment.error"] = new TextFormat { Foreground = Palette["love"] },
            ["comment.warning"] = new TextFormat { Foreground = Palette["gold"] },
            ["comment.todo"] = new TextFormat { Foreground = Palette["rose"], },
            ["comment.hint"] = new TextFormat { Foreground = Palette["iris"] },
            ["comment.info"] = new TextFormat { Foreground = Palette["foam"] },
            ["comment.note"] = new TextFormat { Foreground = Palette["pine"] },

            //// Markup
            //["markup.strong"] = new TextFormat { },
            //["markup.italic"] = new TextFormat { },
            //["markup.strikethrough"] = new TextFormat { },
            //["markup.underline"] = new TextFormat { },

            //["markup.heading"] = new TextFormat { },

            //["markup.quote"] = new TextFormat { },
            //["markup.math"] = new TextFormat { },
            //["markup.environment"] = new TextFormat { },
            //["markup.environment.name"] = new TextFormat { },

            ////"markup.link",
            //["markup.link.label"] = new TextFormat { },
            //["markup.link.url"] = new TextFormat { },

            //["markup.raw"] = new TextFormat { },
            //["markup.raw.block"] = new TextFormat { },

            //["markup.list"] = new TextFormat { },
            //["markup.list.checked"] = new TextFormat { },
            //["markup.list.unchecked"] = new TextFormat { },

            ////Markdown headings
            //["markup.heading.1.markdown"] = new TextFormat { },
            //["markup.heading.2.markdown"] = new TextFormat { },
            //["markup.heading.3.markdown"] = new TextFormat { },
            //["markup.heading.4.markdown"] = new TextFormat { },
            //["markup.heading.5.markdown"] = new TextFormat { },
            //["markup.heading.6.markdown"] = new TextFormat { },
            //["markup.heading.1.marker.markdown"] = new TextFormat { },
            //["markup.heading.2.marker.markdown"] = new TextFormat { },
            //["markup.heading.3.marker.markdown"] = new TextFormat { },
            //["markup.heading.4.marker.markdown"] = new TextFormat { },
            //["markup.heading.5.marker.markdown"] = new TextFormat { },
            //["markup.heading.6.marker.markdown"] = new TextFormat { },

            //["diff.plus"] = new TextFormat { },
            //["diff.minus"] = new TextFormat { },
            //["diff.delta"] = new TextFormat { },

            //["tag"] = new TextFormat { },
            //["tag.attribute"] = new TextFormat { },
            //["tag.delimiter"] = new TextFormat { },
        };


        public HashSet<string> CaptureNames = new HashSet<string> {
        	//  Identifiers
            "variable",
            "variable.builtin",
            "variable.parameter",
            "variable.member",

            "constant",
            "constant.builtin",
            "constant.macro",

            "module",
            "module.builtin",
            "label",

		    // Literals
            "string",
            "string.documentation",
            "string.regexp",
            "string.escape",
            "string.special",
            "string.special.symbol",
            "string.special.url",
            "string.special.path",

            "character",
            "character.special",

            "boolean",
            "number",
            "number.float",

		    // Types
            "type",
            "type.builtin",
            "type.definition",
            "type.qualifier",

            "attribute",
            "property",

		    // Functions
            "function",
            "function.call",
            "function.call.builtin",
            "function.macro",
            "function.method",
            "function.method.call",

            "constructor",
            "operator",

		    // Keywords
            "keyword",
            "keyword.coroutine",
            "keyword.function",
            "keyword.operator",
            "keyword.import",
            "keyword.storage",
            "keyword.repeat",
            "keyword.return",
            "keyword.debug",
            "keyword.exception",
            "keyword.conditional",
            "keyword.conditional.ternary",
            "keyword.directive",
            "keyword.directive.define",

		    // Punctuation
            "punctuation.delimiter",
            "punctuation.bracket",
            "punctuation.special",

		    // Comments
            "comment",
            "comment.documentation",

            "comment.error",
            "comment.warning",
            "comment.todo",
            "comment.hint",
            "comment.info",
            "comment.note",

		    // Markup
            "markup.strong",
            "markup.italic",
            "markup.strikethrough",
            "markup.underline",

            "markup.heading",

            "markup.quote",
            "markup.math",
            "markup.environment",
            "markup.environment.name",

		    //"markup.link",
		    "markup.link.label",
            "markup.link.url",

            "markup.raw",
            "markup.raw.block",

            "markup.list",
            "markup.list.checked",
            "markup.list.unchecked",

		    //Markdown headings
            "markup.heading.1.markdown",
            "markup.heading.2.markdown",
            "markup.heading.3.markdown",
            "markup.heading.4.markdown",
            "markup.heading.5.markdown",
            "markup.heading.6.markdown",
            "markup.heading.1.marker.markdown",
            "markup.heading.2.marker.markdown",
            "markup.heading.3.marker.markdown",
            "markup.heading.4.marker.markdown",
            "markup.heading.5.marker.markdown",
            "markup.heading.6.marker.markdown",

            "diff.plus",
            "diff.minus",
            "diff.delta",

            "tag",
            "tag.attribute",
            "tag.delimiter",
        };
    }
}
