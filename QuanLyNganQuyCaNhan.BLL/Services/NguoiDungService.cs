using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.DAL.Repositories;


namespace QuanLyNganQuyCaNhan.BLL.Services
{
    public class NguoiDungService : INguoiDungService
    {
        private readonly NguoiDungRepository nguoiDungRepository;

        public NguoiDungService(
            NguoiDungRepository nguoiDungRepository)
        {
            this.nguoiDungRepository = nguoiDungRepository;
        }
    }
}
