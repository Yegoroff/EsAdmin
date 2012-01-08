using System;

namespace EsAdmin.ElasticSearch
{
	public interface IConnectionSettings
	{
		string Host { get; }
		int Port { get; }
		int TimeOut { get; }
		string ProxyAddress { get; }
		string Username { get;  }
		string Password { get; }
		string DefaultIndex { get; }
		bool UsesPrettyResponses { get; }
	}
}
