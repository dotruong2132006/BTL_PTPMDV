using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace QuanLyNganQuyCaNhan.DAL.Connections
{
    public class DapperConnectionFactory
    {
        private readonly string chuoiKetNoi;

        public DapperConnectionFactory(string chuoiKetNoi)
        {
            this.chuoiKetNoi = chuoiKetNoi;
        }

        public SqlConnection TaoKetNoi()
        {
            return new SqlConnection(chuoiKetNoi);
        }
    }
}
