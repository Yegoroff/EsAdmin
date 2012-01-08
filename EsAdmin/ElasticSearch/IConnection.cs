using System;

namespace EsAdmin.ElasticSearch
{
    public interface IConnection
    {
        ConnectionStatus Connect();
       
        ConnectionStatus GetSync(string path);
       
        ConnectionStatus PostSync(string path, string data);

        ConnectionStatus PutSync(string path, string data);
        ConnectionStatus DeleteSync(string path);
        ConnectionStatus DeleteSync(string path, string data);
    }
}
