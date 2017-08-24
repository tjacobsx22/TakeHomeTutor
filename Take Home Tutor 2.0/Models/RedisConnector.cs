using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_Home_Tutor_2._0.Models
{
    class RedisConnector
    {
        private static ConnectionMultiplexer _connection;

        public static IDatabase DatabaseConnect()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                _connection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings.GetValues("RedisConnection").FirstOrDefault());
            }

            return _connection.GetDatabase();

        }
    }
}
