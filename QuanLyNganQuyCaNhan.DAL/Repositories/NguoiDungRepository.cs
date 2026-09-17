using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using QuanLyNganQuyCaNhan.DAL.Connections;


namespace QuanLyNganQuyCaNhan.DAL.Repositories
{
    public class NguoiDungRepository
    {
        private readonly DapperConnectionFactory ketNoiCSDL;

        public NguoiDungRepository(
            DapperConnectionFactory ketNoiCSDL)
        {
            this.ketNoiCSDL = ketNoiCSDL;
        }
    }
}
