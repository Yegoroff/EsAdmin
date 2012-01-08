using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EsAdmin.Utils;

namespace EsAdmin.ElasticSearch
{
    public class ConnectionSettings : IConnectionSettings
    {
        private readonly string _username;
        public string Username
        {
            get { return this._username; }
        }
        private readonly string _password;
        public string Password
        {
            get { return this._password; }
        }
        private readonly string _host;
        public string Host
        {
            get { return this._host; }
        }
        private readonly string _proxyAddress;
        public string ProxyAddress
        {
            get { return this._proxyAddress; }
        }
        private readonly int _port;
        public int Port
        {
            get { return this._port; }
        }
        private readonly int _timeOut;
        public int TimeOut
        {
            get { return this._timeOut; }
        }
        private string _defaultIndex;
        public string DefaultIndex
        {
            get
            {
                if (this._defaultIndex.IsNullOrEmpty())
                    throw new NullReferenceException("No default index set on connection!");
                return this._defaultIndex;
            }
            private set { this._defaultIndex = value; }
        }
        public bool UsesPrettyResponses { get; private set; }

        public ConnectionSettings(string host, int port) : this(host, port, 60000, null, null, null) { }
        public ConnectionSettings(string host, int port, int timeout) : this(host, port, timeout, null, null, null) { }
        public ConnectionSettings(string host, int port, int timeout, string proxyAddress, string username, string password)
        {
            if(host.IsNullOrEmpty())
                throw new ArgumentNullException("host");

            var uri = new Uri("http://" + host + ":" + port);


            this._host = host;
            this._password = password;
            this._username = username;
            this._timeOut = timeout;
            this._port = port;
            this._proxyAddress = proxyAddress;
        }

        public ConnectionSettings SetDefaultIndex(string defaultIndex)
        {
            this.DefaultIndex = defaultIndex;
            return this;
        }
        public ConnectionSettings UsePrettyResponses()
        {
            this.UsesPrettyResponses = true;
            return this;
        }
        public ConnectionSettings UsePrettyResponses(bool b)
        {
            this.UsesPrettyResponses = b;
            return this;
        }


    }
}
