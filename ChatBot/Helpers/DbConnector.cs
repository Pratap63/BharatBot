using Microsoft.Data.SqlClient;

namespace ChatBot.Helpers
{
    public class DbConnector
    {
        private readonly string _configuration;

        public DbConnector(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration);
        }

    }
}
