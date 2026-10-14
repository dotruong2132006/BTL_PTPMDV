(function () {
    'use strict';
    angular.module('QuanLyNganQuyCaNhan').controller('BaoMatController', ['$scope', '$rootScope', '$location', '$q', 'NganQuyAPI', 'PhienDangNhap',
        function (scope, root, location, q, api, phien) {
            var bm = this, daDong = false, qrURL;
            bm.duLieu = {}; bm.maKhoiPhuc = []; bm.trangThai2FA = null;
            function thongBao(text, loai) { root.$broadcast('thongBao', text, loai); }
            function loi(e) { if (!daDong) bm.loi = api.loi(e); }
            function docLoiBlob(e) { if (!(e.data instanceof Blob)) return q.when(e); return q.when(e.data.text()).then(function (text) { try { e.data = JSON.parse(text); } catch (_) { /* Thông báo chung nếu máy chủ không trả JSON. */ } return e; }); }
            function huyQR() { if (qrURL) URL.revokeObjectURL(qrURL); qrURL = null; bm.qr = null; }
            bm.taiSoMa = function () { api.get('chung', 'QuenMatKhau/so-ma-con-lai').then(function (d) { if (!daDong) bm.soMa = d; }).catch(loi); };
            bm.kiemTra2FA = function () {
                if (bm.dangQR) return; bm.dangQR = true;
                // API hiện tại trả ảnh QR khi đã bật, hoặc thông báo xác định khi chưa bật.
                api.anh('chung', 'HaiFa/qr').then(function (blob) {
                    if (daDong) return; huyQR(); qrURL = URL.createObjectURL(blob); bm.qr = qrURL; bm.trangThai2FA = true;
                }).catch(function (e) { return docLoiBlob(e).then(function (err) {
                    if (daDong) return;
                    if (err.status === 400 && err.data && err.data.thongBao === 'Tài khoản chưa bật 2FA') { bm.trangThai2FA = false; huyQR(); }
                    else { if (bm.trangThai2FA !== true) bm.trangThai2FA = null; loi(err); }
                }); }).finally(function () { bm.dangQR = false; });
            };
            bm.taoMa = function (form) {
                if (form.$invalid || bm.dangTaoMa || bm.maKhoiPhuc.length || !bm.duLieu.daHieuVaSeLuuMa) return;
                bm.dangTaoMa = true; bm.loi = null;
                api.post('chung', 'QuenMatKhau/tao-ma-khoi-phuc', { matKhauHienTai: bm.duLieu.matKhauHienTai, maOTP: bm.duLieu.maOTP || null, daHieuVaSeLuuMa: true }).then(function (d) {
                    if (daDong || !phien.lay()) return;
                    bm.maKhoiPhuc = d.maKhoiPhuc; bm.canhBao = d.canhBaoQuanTrong; bm.daLuu = false; bm.taiSoMa();
                    thongBao('Đã tạo bộ mã khôi phục. Hãy lưu ngay bộ mã đang hiển thị.');
                }).catch(function (e) { if (!daDong) { loi(e); if (e.status === 0 || e.status === -1 || e.status >= 500) bm.loi = 'Chưa nhận được bộ mã từ máy chủ. Bộ mã cũ có thể đã hết hiệu lực. Hãy kiểm tra kết nối rồi tạo và lưu bộ mã mới bằng mật khẩu hiện tại.'; } }).finally(function () { bm.dangTaoMa = false; bm.duLieu.matKhauHienTai = ''; bm.duLieu.maOTP = ''; });
            };
            function noiDungMa() { return 'QUẢN LÝ NGÂN QUỸ CÁ NHÂN\r\nMÃ KHÔI PHỤC TÀI KHOẢN\r\n\r\n' + bm.maKhoiPhuc.join('\r\n') + '\r\n\r\nMỗi mã dùng một lần. Không chia sẻ. Bộ mã mới thay thế bộ mã cũ.\r\n'; }
            bm.taiMa = function () {
                if (!bm.maKhoiPhuc.length) return;
                var url = URL.createObjectURL(new Blob([noiDungMa()], { type: 'text/plain;charset=utf-8' })), link = document.createElement('a');
                link.href = url; link.download = 'QuanLyNganQuyCaNhan-MaKhoiPhuc.txt'; document.body.appendChild(link); link.click(); link.remove();
                setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
                thongBao('Hãy kiểm tra file đã tải và lưu ở nơi an toàn trước khi xác nhận.');
            };
            bm.saoChep = function () {
                if (!navigator.clipboard) { thongBao('Trình duyệt chưa hỗ trợ sao chép. Hãy tải file mã khôi phục.', 'loi'); return; }
                q.when(navigator.clipboard.writeText(noiDungMa())).then(function () { thongBao('Đã sao chép. Hãy lưu bộ mã ở nơi an toàn.'); }).catch(function () { thongBao('Không sao chép được. Hãy tải file mã khôi phục.', 'loi'); });
            };
            bm.dongBoMa = function () { if (!bm.daLuu) return; bm.maKhoiPhuc = []; bm.canhBao = null; bm.duLieu.daHieuVaSeLuuMa = false; thongBao('Đã đóng bộ mã. Khi cần, tạo bộ mã mới bằng mật khẩu hiện tại.'); };
            bm.bat2FA = function () {
                if (!bm.dongYBat || bm.dang2FA || bm.trangThai2FA !== false) return;
                bm.dang2FA = true; bm.loi = null;
                api.post('chung', 'HaiFa/bat', {}).then(function (d) {
                    if (daDong) return; bm.trangThai2FA = true; bm.maBiMat = d.maBiMat2FA; bm.hienQR = true; bm.daXacNhan = false; bm.kiemTra2FA();
                    thongBao('2FA đã bật. Hãy thêm khóa vào ứng dụng xác thực ngay.');
                }).catch(loi).finally(function () { bm.dang2FA = false; });
            };
            bm.xacNhan2FA = function (form) {
                if (form.$invalid || bm.dang2FA) return; bm.dang2FA = true; bm.loi = null;
                api.post('chung', 'HaiFa/xacnhan', { maOTP: bm.otp2FA }).then(function (d) { if (!daDong) { bm.daXacNhan = true; bm.maBiMat = null; bm.hienQR = false; thongBao(d.thongBao); } }).catch(loi).finally(function () { bm.dang2FA = false; bm.otp2FA = ''; });
            };
            bm.tat2FA = function () {
                if (!bm.dongYTat || bm.dang2FA) return; bm.dang2FA = true; bm.loi = null;
                api.post('chung', 'HaiFa/tat', {}).then(function (d) { if (!daDong) { bm.trangThai2FA = false; bm.maBiMat = null; bm.hienQR = false; bm.dongYTat = false; bm.dongYBat = false; huyQR(); thongBao(d.thongBao); } }).catch(loi).finally(function () { bm.dang2FA = false; });
            };
            function chuaLuu() { return !!phien.lay() && (bm.dangTaoMa || (bm.maKhoiPhuc.length > 0 && !bm.daLuu)); }
            function nhacLuu() { bm.loi = bm.dangTaoMa ? 'Đang tạo bộ mã khôi phục. Vui lòng chờ để nhận và lưu mã.' : 'QUAN TRỌNG: Hãy lưu bộ mã khôi phục và đánh dấu xác nhận đã lưu trước khi rời trang.'; }
            function chanDong(e) { if (chuaLuu()) { e.preventDefault(); e.returnValue = ''; } }
            window.addEventListener('beforeunload', chanDong);
            scope.$on('$routeChangeStart', function (event) {
                if (chuaLuu()) { event.preventDefault(); location.path('/bao-mat'); nhacLuu(); }
            });
            scope.$on('truocDangXuat', function (event) { if (chuaLuu()) { event.preventDefault(); nhacLuu(); } });
            scope.$on('$destroy', function () { daDong = true; huyQR(); bm.maKhoiPhuc = []; bm.maBiMat = null; bm.duLieu = {}; window.removeEventListener('beforeunload', chanDong); });
            bm.taiSoMa(); bm.kiemTra2FA();
        }]);
})();
