using System;
using PlainElastic.Net;
using StringExtensions = EsAdmin.Utils.StringExtensions;

namespace EsAdmin
{
    class EsConnector : IDisposable
    {
        public IElasticConnection Connection { get; private set; }

        public EsConnector(string host, int port)
        {
            Connection = new ElasticConnection{DefaultHost = host, DefaultPort = port};
        }


        #region Implementation of IDisposable

        public void Dispose()
        {
            // nothing to dispose yet.
        }

        #endregion

        public string Execute(string textToExecute)
        {
            /*            
             GET /companies/icompanies/_mapping
             {
                  "icompanies": {
                    "properties": {
                      "businessDetails.industry": {
                        "type": "string",
                        "index": "not_analyzed"
                      }
                    }
                  }
                }
            */

            textToExecute = textToExecute.Trim(' ', '\r', '\n', '\t');

            var splittedText = textToExecute.Trim(' ', '\r', '\n').Split(new[] {" ", "\r\n", "\t"},
                                                                         StringSplitOptions.RemoveEmptyEntries);
            string action = splittedText[0];
            string path = splittedText[1];
            int endOfFirstLine = textToExecute.IndexOf("\r\n");
            string body = "";
            if (endOfFirstLine > 0)
                body = textToExecute.Remove(0, endOfFirstLine + 2);

            try
            {
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
                        throw new ArgumentException(StringExtensions.F("Invalid Action {0} detected", action),
                                                    "textToExecute");
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
