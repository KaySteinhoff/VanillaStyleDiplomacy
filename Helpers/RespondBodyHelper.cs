using System.Collections.Generic;
using System.IO;
using System.Net;

namespace VanillaStyleDiplomacy.Helpers
{
    public class RespondBodyHelper
    {
        private string uri = "";

        public RespondBodyHelper()
        {

        }

        public RespondBodyHelper(string uri)
        {
            this.uri = uri;
        }

        public byte[] FetchBody(string uri, params KeyValuePair<string, string>[] headers)
        {
            WebClient client = new WebClient();
            for (int i = 0; i < headers.Length; ++i)
                client.Headers.Add(headers[i].Key, headers[i].Value);
            byte[] result = client.DownloadData(uri);
            client.Dispose();
            return result;
        }

        public byte[] FetchBody(params KeyValuePair<string, string>[] headers)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new InvalidDataException($"No Uri given.");
            return FetchBody(uri, headers);
        }
    }
}