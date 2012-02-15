using System;
using PlainElastic.Net;

namespace EsAdmin
{
    class EsConnector
    {
        public ElasticConnection Connection { get; private set; }

        public EsConnector(string host, int port)
        {
            Connection = new ElasticConnection(host, port);
        }


        public string Execute(string textToExecute)
        {
            textToExecute = textToExecute.Trim(' ', '\r', '\n', '\t');

            var splittedText = textToExecute.Trim(' ', '\r', '\n').Split(new[] { " ", "\r\n", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            string action = splittedText[0];
            string path = splittedText[1];
            path = UrlBuilder.AddParameter(path, "pretty", "true");

            int endOfFirstLine = textToExecute.IndexOf("\r\n");
            string body = "";
            if (endOfFirstLine > 0)
                body = textToExecute.Remove(0, endOfFirstLine + 2);            

            switch (action.ToUpper())
            {
                case "GET":
                    if (string.IsNullOrWhiteSpace(body))
                        return Connection.Get(path);
                    return Connection.Post(path, body);

                case "POST":
                    return Connection.Post(path, body);

                case "PUT":
                    return Connection.Put(path, body);

                case "DELETE":
                    return Connection.Delete(path, body);

                default:
                    throw new ArgumentException("Invalid Action {0} detected".F(action),"textToExecute");
            }


        }
    }
}
