using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Evolutionlab.ExportFic
{
    public static class StringExtensions
    {

        #region stringhe generali
        public static string ToString(object obj)
        {
            return (obj ?? "").ToString();
        }

        public static string ToNotNull(this string text, string defaultValue = "")
        {
            return string.IsNullOrWhiteSpace(text) ? defaultValue : text;
        }

        public static string ToNotNull(this object text, string defaultValue = "")
        {

            return text != null
                ? string.IsNullOrWhiteSpace(text.ToString())
                    ? defaultValue
                    : text.ToString()
                : defaultValue;
        }

        public static string ToUpperFirst(this string text)
        {
            text = text.Trim().ToLower();
            return Char.ToUpper(text[0]) +
                   ((text.Length > 1) ? text.Substring(1) : String.Empty);
        }

        public static string ToTitleCase(this string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string TruncateWord(this string value, int maxLength)
        {
            if (value == null || value.Trim().Length <= maxLength)
                return value;

            string ellipse = "...";
            char[] truncateChars = { ' ', ',', '.', '%' };
            int index = value.Trim().LastIndexOfAny(truncateChars);

            while ((index + ellipse.Length) > maxLength)
                index = value.Substring(0, index).Trim().LastIndexOfAny(truncateChars);

            if (index > 0)
                return value.Substring(0, index) + ellipse;

            return value.Substring(0, maxLength - ellipse.Length) + ellipse;
        }

        /// <summary>
        /// Restituisce una stringa sostituendo tutti i valori trovati
        /// nelle chiavi del dizionario con i relativi valori
        /// </summary>
        /// <param name="text">Testo iniziale</param>
        /// <param name="parametri">Dizionario di parametri da sostituire</param>
        /// <returns></returns>
        public static string Replace(this string text, Dictionary<string, string> parametri)
        {
            if (parametri != null)
                text = parametri.Aggregate(text, (current, parametro) => current.Replace("$" + parametro.Key.ToUpper() + "$", parametro.Value));

            return text;
        }

        public static string TrimEnd(this string source, string value)
        {
            return !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }


        public static bool IsImage(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            source = source.ToLower();
            return source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") || source.EndsWith(".gif") || source.EndsWith(".bmp") || source.EndsWith(".tif") || source.EndsWith(".tiff");
        }

        public static string ToEmailList(this string source)
        {
            return !string.IsNullOrWhiteSpace(source) ? source.Replace(" ", "").Replace(",", ";") : "";
        }

        #endregion

        #region url e slug
        public static string ToUrl(this string text)
        {
            text = text.Trim();
            if (!text.StartsWith("/") && !text.StartsWith("http://") && !text.StartsWith("https://"))
            {
                return "http://" + text;
            }

            return text;
        }

        public static string ToSlug(this string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return "";

            string str = phrase.RimuoviAccenti().ToLower();
            str = str.Replace("-", "");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // invalid chars           
            str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space   
            str = str.Substring(0, str.Length <= 85 ? str.Length : 85).Trim(); // cut and trim it   
            str = Regex.Replace(str, @"\s", "-"); // hyphens   

            return str;
        }

        public static string ToYoutubeEmbed(this string text, int width = 480, int height = 390, bool autoplay = false, string title = "Video Youtube")
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
            string replacement = $"<iframe title=\"{title}\" width=\"{width}\" height=\"{height}\" src='http://www.youtube.com/embed/$1?autoplay={(autoplay ? 1 : 0)}' frameborder='0' allowfullscreen='1'></iframe>";

            var rgx = new Regex(pattern);
            return rgx.Replace(text, replacement);
        }

        private static string RimuoviAccenti(this string txt)
        {
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string ToFirstQueryStringParam(this string txt)
        {
            txt = txt.TrimStart('&');
            return txt != ""
                ? "?" + txt
                : "";
        }

        #endregion

        #region html e javascript

        public static string ToJavaScript(this string text)
        {
            if (text != null)
            {
                text = text.Replace("<br>", "\\n").Replace("'", "\\'");
                return Regex.Replace(text, @"<(.|\n)*?>", String.Empty);
            }
            return "";
        }

        #endregion

        #region numeri, prezzi e date

        public static string ToOra(this int? numero, string prefisso = "")
        {
            if (numero.HasValue)
            {
                double ris = numero.Value.ToDbl() / 100;

                var output = ris.ToString().Split(',');

                if (output.Length > 1)
                {
                    var ore = output[0];
                    var minuti = output[1];

                    if (ore.Length < 2)
                        ore = "0" + ore;

                    if (minuti.Length < 2)
                        minuti = minuti + "0";

                    return prefisso + ore + ":" + minuti;
                }

                return prefisso + ris + ":00";
            }

            return "";
        }

        public static int ToInt(this string numero, int defVal = 0)
        {
            int num;
            if (!Int32.TryParse(numero, out num))
            {
                num = defVal;
            }

            return num;
        }

        public static int ToInt(this object numero, int defVal = 0)
        {
            return numero?.ToString().ToInt() ?? defVal;
        }


        public static double ToDbl(this string numero, double defVal = 0)
        {
            double num;
            if (!Double.TryParse(numero, out num))
            {
                num = defVal;
            }

            return num;
        }

        public static double ToDbl(this object numero, double defVal = 0)
        {
            return numero?.ToString().ToDbl() ?? defVal;
        }

        public static string ToStringPrice(this decimal prezzo)
        {
            return prezzo == 0
                ? "n/a"
                : prezzo.ToString("G29") + " €";
        }

        public static bool ToBool(this string boolean, bool defVal = false)
        {
            bool bl;
            if (!Boolean.TryParse(boolean, out bl))
            {
                bl = defVal;
            }

            return bl;
        }

        public static bool ToBool(this object boolean, bool defVal = false)
        {
            return boolean?.ToString().ToBool() ?? defVal;
        }


        public static DateTime? ToDateTime(this string data, DateTime? defVal = null)
        {
            return !DateTime.TryParse(data, out DateTime myDate) ? defVal : myDate;
        }

        #endregion

    }
}
