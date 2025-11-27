using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TietokoneTiedotApp
{
    public static class PastebinUploader
    {
        // Hae oma avain: https://pastebin.com/doc_api
        private const string ApiDevKey = "rSkK-cjwRztSwRSMWL6Rh-uz_TbgeGl-"; 
        private const string PastebinUrl = "https://pastebin.com/api/api_post.php";

        public static async Task<string> LähetäRaportti(List<string> rivit, string otsikko = "Raportti")
        {
            using var client = new HttpClient();

            var sisältö = string.Join(Environment.NewLine, rivit);
            var dict = new Dictionary<string, string>
    {
        { "api_dev_key", ApiDevKey },
        { "api_option", "paste" },
        { "api_paste_code", sisältö },
        { "api_paste_name", otsikko },
        { "api_paste_expire_date", "1W" },
        { "api_paste_format", "text" },
        { "api_paste_private", "1" }
    };

            var content = new FormUrlEncodedContent(dict);
            var vastaus = await client.PostAsync(PastebinUrl, content);
            var vastausteksti = await vastaus.Content.ReadAsStringAsync();

            if (!vastaus.IsSuccessStatusCode)
                throw new Exception($"Pastebin error: {vastausteksti}");

            return vastausteksti;
        }

    }
}
