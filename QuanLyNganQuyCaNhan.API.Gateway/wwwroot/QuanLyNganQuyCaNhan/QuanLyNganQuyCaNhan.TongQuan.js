(function () {
    'use strict';
    angular.module('QuanLyNganQuyCaNhan').controller('TongQuanController', ['$scope', '$q', 'NganQuyAPI', function (scope, q, api) {
        var tq = this, now = new Date(), lanTai = 0, daDong = false;
        tq.thang = now.getMonth() + 1; tq.nam = now.getFullYear(); tq.cacThang = Array.from({ length: 12 }, function (_, i) { return i + 1; });
        tq.mauSac = ['#3787fc', '#1ac38c', '#ffbb39', '#9a79ec', '#ff7a8e', '#47cbd6', '#8d9fbd', '#5268d0'];
        tq.moc = [0, 1, 2, 3, 4]; tq.tongQuan = {}; tq.thangDuLieu = []; tq.coCauChi = []; tq.giaoDich = []; tq.nganSach = [];
        function taoBieuDo(data) {
            tq.thangDuLieu = data;
            tq.max = Math.max(1, Math.max.apply(null, data.map(function (d) { return Math.max(d.tongThu, d.tongChi); })) * 1.18);
            tq.x = function (i) { return 48 + i * 49; }; tq.y = function (value) { return 205 - (Number(value) || 0) / tq.max * 170; };
            function duong(key) { return data.map(function (d, i) { return (i ? 'L' : 'M') + tq.x(i) + ' ' + tq.y(d[key]); }).join(' '); }
            tq.duongThu = duong('tongThu'); tq.duongChi = duong('tongChi');
            tq.vungChi = data.length ? tq.duongChi + ' L' + tq.x(11) + ' 205 L48 205 Z' : '';
        }
        function taoCoCau(data) {
            tq.coCauChi = data.filter(function (d) { return d.loaiGiaoDich === 'CHI'; });
            tq.tongChiDanhMuc = tq.coCauChi.reduce(function (s, d) { return s + d.tongTien; }, 0);
            var pos = 0, parts = [];
            tq.coCauChi.forEach(function (d, i) { d.mau = tq.mauSac[i % tq.mauSac.length]; var next = pos + (tq.tongChiDanhMuc ? d.tongTien / tq.tongChiDanhMuc * 100 : 0);
                parts.push(d.mau + ' ' + pos + '% ' + next + '%'); pos = next; });
            tq.donutStyle = { background: parts.length ? 'conic-gradient(' + parts.join(',') + ')' : '#edf2fa' };
        }
        tq.tai = function () {
            if (!tq.thang || !tq.nam || tq.nam < 2000 || tq.nam > 9998) { tq.loi = 'Chọn tháng và năm hợp lệ.'; return; }
            var id = ++lanTai; tq.dangTai = true; tq.loi = null;
            var tuNgay = tq.nam + '-' + String(tq.thang).padStart(2, '0') + '-01', cuoiThang = api.ngayLocal(new Date(tq.nam, tq.thang, 0)).slice(0, 10);
            q.all([api.get('user', 'ThongKe/tong-quan', { thang: tq.thang, nam: tq.nam }), api.get('user', 'ThongKe/theo-thang', { nam: tq.nam }),
                api.get('user', 'ThongKe/theo-danh-muc', { thang: tq.thang, nam: tq.nam }), api.get('user', 'GiaoDich', { tuNgay: tuNgay, denNgay: cuoiThang, trang: 1, kichThuocTrang: 5 }),
                api.get('user', 'NganSach', { thang: tq.thang, nam: tq.nam })]).then(function (duLieu) {
                    if (id !== lanTai || daDong) return;
                    tq.tongQuan = duLieu[0]; taoBieuDo(duLieu[1]); taoCoCau(duLieu[2]); tq.giaoDich = duLieu[3].danhSach; tq.nganSach = duLieu[4];
                }).catch(function (e) { if (id === lanTai && !daDong) tq.loi = api.loi(e); }).finally(function () { if (id === lanTai) tq.dangTai = false; });
        };
        tq.tai(); scope.$on('$destroy', function () { daDong = true; });
    }]);
})();
