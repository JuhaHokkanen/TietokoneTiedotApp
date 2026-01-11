using System;
using System.IO;
using System.Text;

namespace TietokoneTiedotApp
{
    public static class HtmlRaportti
    {
        public static string TallennaHtmlTiedosto(TietokoneTiedot tiedot)
        {
            string tiedostonimi = $"TietokoneRaportti_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string kansio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string polku = Path.Combine(kansio, tiedostonimi);

            // HTML builder
            var sb = new StringBuilder();

            sb.Append("""
<!DOCTYPE html>
<html lang="fi">
<head>
<meta charset="UTF-8">
<title>Tietokoneen tiedot</title>
<style>
    body {
        font-family: 'Segoe UI', sans-serif;
        background: #121212;
        color: #f0f0f0;
        padding: 20px;
        line-height: 1.6;
    }
    h1 {
        color: #ffc400;
    }
    ul {
        list-style-type: none;
        padding-left: 0;
    }
    li {
        margin-bottom: 8px;
        padding: 6px;
        background: #1f1f1f;
        border-radius: 6px;
    }
</style>
</head>
<body>
""");

            sb.AppendLine($"<h1>Tietokoneen tiedot ({DateTime.Now:dd.MM.yyyy HH:mm})</h1>");
            sb.AppendLine("<ul>");

            foreach (var rivi in tiedot.HaeKaikkiTiedot())
            {
                // HtmlEncode varmistaa, että erikoismerkit (<, >, &) eivät riko HTML-rakennetta
                string encoded = System.Net.WebUtility.HtmlEncode(rivi);
                sb.AppendLine($"<li>{encoded}</li>");
            }

            sb.Append("""
</ul>
</body>
</html>
""");

            // Tallenna tiedosto
            File.WriteAllText(polku, sb.ToString(), Encoding.UTF8);

            return polku;
        }
    }
}
