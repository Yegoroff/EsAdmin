using System;

namespace EsAdmin.ElasticSearch
{
    public interface IConnection
    {
        ConnectionStatus Connect();
       
        ConnectionStatus Get(string path);
       
        ConnectionStatus Post(string path, string data);

        ConnectionStatus Put(string path, string data);
        ConnectionStatus Delete(string path);
        ConnectionStatus Delete(string path, string data);
    }
}
