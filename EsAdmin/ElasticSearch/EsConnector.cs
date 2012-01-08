using System;
using EsAdmin.ElasticSearch;
using EsAdmin.Utils;

namespace EsAdmin
{
    class EsConnector : IDisposable
    {
        public IConnection Connection { get; private set; }

        public EsConnector(string host, int port, string index)
        {
            var settings = new ConnectionSettings(host, port)
                .UsePrettyResponses(true)
                .SetDefaultIndex(index);

            Connection = new Connection(settings);

            if (!Connection.Connect().Success)
                throw new InvalidOperationException("No Elastic Search connection esatblished");
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

            textToExecute = textToExecute.Trim(' ', '\r', '\n');

            var splittedText = textToExecute.Trim(' ', '\r', '\n').Split(new[] { " ", "\r\n", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            string action = splittedText[0];
            string path = splittedText[1];
            int endOfFirstLine = textToExecute.IndexOf("\r\n");
            string body = "";
            if (endOfFirstLine > 0)
                body = textToExecute.Remove(0, endOfFirstLine + 2);            
                
            ConnectionStatus status = null;

            switch (action.ToUpper())
            {
                case "GET":
                    if (string.IsNullOrWhiteSpace(body))
                        status = Connection.GetSync(path);
                    else
                        status = Connection.PostSync(path, body);
                    break;
                case "POST":
                    status = Connection.PostSync(path, body);
                    break;
                case "PUT":
                    status = Connection.PutSync(path, body);
                    break;
                case "DELETE":
                    status = Connection.DeleteSync(path, body);
                    break;
                default:
                    throw new ArgumentException("Invalid Action {0} detected".F(action),
                        "textToExecute");
            }

            if (!status.Success)
                return status.Error.ExceptionMessage;

            return status.Result;
        }
    }
}
