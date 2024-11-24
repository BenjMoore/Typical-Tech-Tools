using Ganss.Xss;
namespace TypicalTechTools
{
    public class Sanitizer
    {
        public static string Sanitize(string inputHtml)
        {
            // Create a simple sanitizer instance
            var sanitizer = new HtmlSanitizer();

            return sanitizer.Sanitize(inputHtml);
        }
    }
}
