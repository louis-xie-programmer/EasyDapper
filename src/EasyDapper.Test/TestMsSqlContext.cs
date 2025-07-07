using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyDapper.Test
{
    public class TestMsSqlContext : IDisposable
    {
        public SqlConnection Context
        {
            get
            {
                if (_context == null)
                {
                    var ConnectionString = "Server=.;Database=test;User Id=easy;Password=easy;Encrypt=False";
                    _context = new SqlConnection(ConnectionString);
                }
                return _context;
            }
        }

        private SqlConnection? _context;

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
