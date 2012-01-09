using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace EsAdmin.ElasticSearch
{
    internal class Connection : IConnection
    {
        private IConnectionSettings ConnectionSettings { get; set; }

        public Connection(IConnectionSettings settings)
        {
            this.ConnectionSettings = settings;
        }


        public ConnectionStatus Connect()
        {
            return Get("");
        }            

        public ConnectionStatus Get(string path)
        {
            var connection = this.CreateConnection(path, "GET");
            connection.Timeout = this.ConnectionSettings.TimeOut;
            WebResponse response = null;
            try
            {
                response = connection.GetResponse();
                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                return new ConnectionStatus(result);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                    return new ConnectionStatus(new ConnectionError(e) { Type = ConnectionErrorType.Server, ExceptionMessage = "Timeout"});

                if (e.Status != WebExceptionStatus.Success
                    && e.Status != WebExceptionStatus.ProtocolError)
                    return new ConnectionStatus(new ConnectionError(e) { Type = ConnectionErrorType.Server });

                return new ConnectionStatus(new ConnectionError(e));
            }
            catch (Exception e) { return new ConnectionStatus(new ConnectionError(e)); }
            finally
            {
                if (response != null)
                    response.Close();
            }

        }


        public ConnectionStatus Post(string path, string data)
        {
            return this.PostOrPutSync(path, data, "POST");
        }
        public ConnectionStatus Put(string path, string data)
        {
            return this.PostOrPutSync(path, data, "PUT");
        }


        private ConnectionStatus PostOrPutSync(string path, string data, string method)
        {
            var connection = this.CreateConnection(path, method);
            connection.Timeout = this.ConnectionSettings.TimeOut;
            Stream postStream = null;
            WebResponse response = null;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                connection.ContentLength = buffer.Length;
                postStream = connection.GetRequestStream();
                postStream.Write(buffer, 0, buffer.Length);
                postStream.Close();
                response = connection.GetResponse();
                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                return new ConnectionStatus(result);
            }
            catch (WebException e)
            {
                ConnectionError error;
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    error = new ConnectionError(e) { HttpStatusCode = HttpStatusCode.InternalServerError };
                }
                else
                {
                    error = new ConnectionError(e);
                }
                return new ConnectionStatus(error);
            }
            catch (Exception e) { return new ConnectionStatus(new ConnectionError(e)); }
            finally
            {
                if (postStream != null)
                    postStream.Close();
                if (response != null)
                    response.Close();
            }
        }
        public ConnectionStatus Delete(string path)
        {
            var connection = this.CreateConnection(path, "DELETE");
            connection.Timeout = this.ConnectionSettings.TimeOut;
            WebResponse response = null;
            try
            {
                response = connection.GetResponse();
                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                return new ConnectionStatus(result);
            }
            catch (WebException e)
            {
                ConnectionError error;
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    error = new ConnectionError(e) { HttpStatusCode = HttpStatusCode.InternalServerError };
                }
                else
                {
                    error = new ConnectionError(e);
                }
                return new ConnectionStatus(error);
            }
            catch (Exception e) { return new ConnectionStatus(new ConnectionError(e)); }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
        public ConnectionStatus Delete(string path, string data)
        {
            var connection = this.CreateConnection(path, "DELETE");
            connection.Timeout = this.ConnectionSettings.TimeOut;
            Stream postStream = null;
            WebResponse response = null;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                connection.ContentLength = buffer.Length;
                postStream = connection.GetRequestStream();
                postStream.Write(buffer, 0, buffer.Length);
                postStream.Close();
                response = connection.GetResponse();
                var result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                return new ConnectionStatus(result);
            }
            catch (WebException e)
            {
                ConnectionError error;
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    error = new ConnectionError(e) { HttpStatusCode = HttpStatusCode.InternalServerError };
                }
                else
                {
                    error = new ConnectionError(e);
                }
                return new ConnectionStatus(error);
            }
            catch (Exception e) { return new ConnectionStatus(new ConnectionError(e)); }
            finally
            {
                if (postStream != null)
                    postStream.Close();
                if (response != null)
                    response.Close();
            }
        }

        private HttpWebRequest CreateConnection(string path, string method)
        {
            var url = this._CreateUriString(path);
            if (this.ConnectionSettings.UsesPrettyResponses)
            {
                var uri = new Uri(url);
                url += ((string.IsNullOrEmpty(uri.Query)) ? "?" : "&") + "pretty=true";
            }
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Timeout = 1000 * 60; // 1 minute timeout.
            myReq.ReadWriteTimeout = 1000 * 60; // 1 minute timeout.
            myReq.Method = method;
            
            if (!string.IsNullOrEmpty(this.ConnectionSettings.ProxyAddress))
            {
                var proxy = new WebProxy();
                var uri = new Uri(this.ConnectionSettings.ProxyAddress);
                var credentials = new NetworkCredential(this.ConnectionSettings.Username, this.ConnectionSettings.Password);
                proxy.Address = uri;
                proxy.Credentials = credentials;
                myReq.Proxy = proxy;
            }
            return myReq;
        }

        private string _CreateUriString(string path)
        {
            var s = this.ConnectionSettings;
            if (!path.StartsWith("/"))
                path = "/" + path;
            return string.Format("http://{0}:{1}{2}", s.Host, s.Port, path);
        }

    }
}
