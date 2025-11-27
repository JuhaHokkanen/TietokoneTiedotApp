using System;
using System.IO;
using System.Text;

namespace TietokoneTiedotApp
{
    public static class HtmlRaportti
    {
        public static void TallennaHtmlTiedosto(TietokoneTiedot tiedot)
        {
            var polku = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"TietokoneRaportti_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"fi\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<title>Tietokoneen tiedot</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', sans-serif; background: #1e1e1e; color: #ffffff; padding: 20px; }");
            sb.AppendLine("h1 { color: #FFD700; }");
            sb.AppendLine("ul { list-style-type: none; padding: 0; }");
            sb.AppendLine("li { margin-bottom: 6px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine($"<h1>Tietokoneen tiedot ({DateTime.Now:dd.MM.yyyy HH:mm})</h1>");
            sb.AppendLine("<ul>");

            foreach (var rivi in tiedot.HaeKaikkiTiedot())
            {
                sb.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(rivi)}</li>");
            }

            sb.AppendLine("</ul>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(polku, sb.ToString());
        }
    }
}
