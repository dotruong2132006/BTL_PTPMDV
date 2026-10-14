(function () {
    'use strict';
    var ungDung = angular.module('QuanLyNganQuyCaNhan');
    // File được đưa vào Angular qua directive, không dùng trình xử lý sự kiện viết trong HTML.
    ungDung.directive('chonFileNganQuy', function () {
        return { restrict: 'A', scope: { chonFileNganQuy: '&' }, link: function (scope, element) {
            function chon() { var file = element[0].files[0]; scope.$evalAsync(function () { scope.chonFileNganQuy({ file: file }); }); }
            element.on('change', chon); scope.$on('$destroy', function () { element.off('change', chon); });
        } };
    });
    ungDung.controller('QuanLyController', ['$scope', '$rootScope', '$route', '$q', '$interval', '$timeout', '$location', 'NganQuyAPI', 'PhienDangNhap',
        function (scope, root, route, q, interval, timeout, location, api, phien) {
            var ql = this, daDong = false, lanTai = 0, lanThongBao = 0, lichPDF, maPDF, khoaPDF, phanTuTruocModal, luaChonCache = {};
            var homNay = new Date(), loai = [{ ma: 'THU', ten: 'Thu nhập' }, { ma: 'CHI', ten: 'Chi tiêu' }];
            function truong(ma, ten, kieu, batBuoc, them) { return angular.extend({ ma: ma, ten: ten, kieu: kieu || 'text', batBuoc: !!batBuoc }, them || {}); }
            var ten = truong('tenDanhMuc', 'Tên danh mục', 'text', true, { dai: 100 });
            var loaiDanhMuc = truong('loaiDanhMuc', 'Loại danh mục', 'select', true, { luaChon: loai });
            var moTa = truong('moTa', 'Mô tả', 'textarea', false, { dai: 255, rong: true });
            var cacTrang = {
                'vi': { tieuDe: 'Ví tiền của bạn', moTa: 'Theo dõi số dư được đồng bộ từ các giao dịch.', api: 'Vi', id: 'maVi', ten: 'ví',
                    truong: [truong('tenVi', 'Tên ví', 'text', true, { dai: 100 }), truong('trangThai', 'Ví đang hoạt động', 'checkbox', false, { chiSua: true })] },
                'danh-muc': { tieuDe: 'Danh mục thu chi', moTa: 'Danh mục cá nhân và danh mục hệ thống dùng chung.', api: 'DanhMuc', id: 'maDanhMuc', ten: 'danh mục',
                    truong: [ten, loaiDanhMuc, moTa], cot: [{ ma: 'maDanhMuc', ten: 'Mã' }, { ma: 'tenDanhMuc', ten: 'Danh mục' }, { ma: 'loaiDanhMuc', ten: 'Loại', kieu: 'loai' }, { ma: 'moTa', ten: 'Mô tả' }] },
                'giao-dich': { tieuDe: 'Sổ thu chi', moTa: 'Mỗi giao dịch được cập nhật cùng số dư ví.', api: 'GiaoDich', id: 'maGiaoDich', ten: 'giao dịch', phanTrang: true,
                    truong: [truong('loaiGiaoDich', 'Loại giao dịch', 'select', true, { luaChon: loai }), truong('maVi', 'Ví tiền', 'select', true, { nguon: 'vi' }),
                        truong('maDanhMuc', 'Danh mục', 'select', true, { nguon: 'danh-muc' }), truong('soTien', 'Số tiền (VND)', 'tien', true),
                        truong('ngayGiaoDich', 'Thời điểm giao dịch', 'datetime-local', true), truong('noiDung', 'Nội dung', 'textarea', false, { dai: 255, rong: true })],
                    cot: [{ ma: 'ngayGiaoDich', ten: 'Thời điểm', kieu: 'ngay' }, { ma: 'noiDung', ten: 'Nội dung' }, { ma: 'tenDanhMuc', ten: 'Danh mục' }, { ma: 'tenVi', ten: 'Ví tiền' }, { ma: 'soTien', ten: 'Số tiền', kieu: 'tien' }] },
                'ngan-sach': { tieuDe: 'Ngân sách tháng', moTa: 'Đặt giới hạn chi tiêu và nhận cảnh báo ngay trên màn hình.', api: 'NganSach', id: 'maNganSach', ten: 'ngân sách',
                    truong: [truong('maDanhMuc', 'Danh mục chi tiêu', 'select', true, { nguon: 'danh-muc' }), truong('soTienGioiHan', 'Giới hạn (VND)', 'tien', true),
                        truong('thang', 'Tháng', 'number', true, { min: 1, max: 12 }), truong('nam', 'Năm', 'number', true, { min: 2000, max: 9998 }),
                        truong('phanTramCanhBao', 'Cảnh báo khi đạt (%)', 'number', true, { min: 0.01, max: 100, buoc: 0.01 })] },
                'nhac-nho': { tieuDe: 'Nhắc nhở theo lịch', moTa: 'Thông báo xuất hiện tại biểu tượng chuông khi đến lịch nhắc.', api: 'NhacNho', id: 'maNhacNho', ten: 'nhắc nhở',
                    truong: [truong('tieuDe', 'Tiêu đề', 'text', true, { dai: 200, rong: true }), truong('noiDung', 'Nội dung', 'textarea', false, { dai: 500, rong: true }),
                        truong('ngayNhac', 'Thời điểm nhắc', 'datetime-local', true, { rong: true }), truong('lapLai', 'Lặp lại theo lịch', 'checkbox'),
                        truong('kieuLapLai', 'Chu kỳ', 'select', true, { khiLapLai: true, luaChon: [{ ma: 'HANG_NGAY', ten: 'Hằng ngày' }, { ma: 'HANG_TUAN', ten: 'Hằng tuần' }, { ma: 'HANG_THANG', ten: 'Hằng tháng' }] }),
                        truong('trangThai', 'Đang bật nhắc nhở', 'checkbox')] },
                'nguoi-dung': { tieuDe: 'Quản lý người dùng', moTa: 'Quản lý tài khoản và trạng thái hoạt động.', host: 'admin', api: 'NguoiDung', id: 'maNguoiDung', ten: 'người dùng',
                    truong: [truong('tenDangNhap', 'Tên đăng nhập', 'text', true, { chiThem: true, dai: 50 }), truong('hoTen', 'Họ tên', 'text', true, { dai: 100 }),
                        truong('email', 'Email', 'email', true, { dai: 100 }), truong('soDienThoai', 'Số điện thoại', 'text', false, { dai: 20 }),
                        truong('matKhau', 'Mật khẩu ban đầu', 'password', true, { chiThem: true, minDai: 8, dai: 72 }), truong('trangThai', 'Tài khoản đang hoạt động', 'checkbox', false, { chiSua: true })],
                    cot: [{ ma: 'maNguoiDung', ten: 'Mã' }, { ma: 'hoTen', ten: 'Họ tên' }, { ma: 'tenDangNhap', ten: 'Tên đăng nhập' }, { ma: 'email', ten: 'Email' }, { ma: 'trangThai', ten: 'Hoạt động', kieu: 'bool' }, { ma: 'daBat2FA', ten: '2FA', kieu: 'bool' }] },
                'danh-muc-he-thong': { tieuDe: 'Danh mục hệ thống', moTa: 'Danh mục dùng chung cho mọi người dùng.', host: 'admin', api: 'DanhMucHeThong', id: 'maDanhMuc', ten: 'danh mục hệ thống',
                    truong: [ten, loaiDanhMuc, moTa], cot: [{ ma: 'maDanhMuc', ten: 'Mã' }, { ma: 'tenDanhMuc', ten: 'Danh mục' }, { ma: 'loaiDanhMuc', ten: 'Loại', kieu: 'loai' }, { ma: 'moTa', ten: 'Mô tả' }] },
                'nhat-ky': { tieuDe: 'Nhật ký hoạt động', moTa: 'Tra cứu lịch sử thay đổi dữ liệu trong hệ thống.', host: 'admin', api: 'NhatKyAudit', id: 'maAudit', phanTrang: true, chiDoc: true,
                    cot: [{ ma: 'thoiGian', ten: 'Thời gian', kieu: 'ngay' }, { ma: 'maNguoiDung', ten: 'Người dùng' }, { ma: 'hanhDong', ten: 'Hành động' }, { ma: 'tenBang', ten: 'Bảng' }, { ma: 'maBanGhi', ten: 'Bản ghi' }, { ma: 'diaChiIP', ten: 'Địa chỉ IP' }] },
                'bao-cao': { tieuDe: 'Báo cáo & xuất nhập', moTa: 'Báo cáo tài chính tháng và dữ liệu giao dịch của bạn.', api: 'BaoCaoThang', id: 'maBaoCao', phanTrang: true, chiDoc: true },
                'suc-khoe': { tieuDe: 'Trạng thái hệ thống', moTa: 'Kiểm tra trạng thái kết nối cơ sở dữ liệu.', host: 'admin', api: 'Health', chiDoc: true }
            };
            ql.trang = route.current.maTrang; ql.cauHinh = cacTrang[ql.trang]; ql.danhSach = []; ql.vi = []; ql.danhMuc = []; ql.trangSo = 1; ql.kichThuoc = 20;
            ql.loc = { sapXepTheo: 'NgayGiaoDich', chieuSapXep: 'DESC' }; ql.timKiem = ql.trang === 'giao-dich' && scope.app ? scope.app.timKiem : '';
            ql.xuat = { dinhDang: 'xlsx' }; ql.baoCao = { thang: homNay.getMonth() + 1, nam: homNay.getFullYear() };
            ql.thongBao = []; ql.trangThongBao = 1;
            ql.trangThaiLuaChon = [{ ma: true, ten: 'Đang bật' }, { ma: false, ten: 'Đang tắt' }];
            ql.mauTien = /^(?:0|[1-9][0-9]{0,15})(?:\.[0-9]{1,2})?$/;
            function thongBao(text, loaiTin) { root.$broadcast('thongBao', text, loaiTin); }
            function loi(e) { if (!daDong) ql.loi = api.loi(e); }
            function thamSo() {
                var p = { trang: ql.trangSo, kichThuocTrang: ql.kichThuoc };
                Object.keys(ql.loc).forEach(function (key) { var value = ql.loc[key]; if (value !== '' && value !== null && value !== undefined) p[key] = value instanceof Date ? api.ngayLocal(value) : value; });
                return p;
            }
            function xuLyDanhSach(d) {
                ql.danhSach = ql.cauHinh.phanTrang ? d.danhSach : d; ql.tong = ql.cauHinh.phanTrang ? d.tongSoBanGhi : d.length;
                if (ql.trang === 'bao-cao') datLichPDF();
            }
            ql.tai = function () {
                if (ql.loiLuaChon) return taiLuaChon();
                var lan = ++lanTai; ql.dangTai = true; ql.loi = null;
                return api.get(ql.cauHinh.host || 'user', ql.cauHinh.api, thamSo()).then(function (d) {
                    if (daDong || lan !== lanTai) return;
                    if (ql.trang === 'suc-khoe') ql.sucKhoe = d; else xuLyDanhSach(d);
                    if (ql.trang === 'nhac-nho') ql.taiThongBao();
                }).catch(function (e) {
                    if (daDong || lan !== lanTai) return;
                    if (ql.trang === 'suc-khoe' && e.status === 503 && e.data && e.data.thanhPhan) ql.sucKhoe = e.data; else loi(e);
                }).finally(function () { if (!daDong && lan === lanTai) ql.dangTai = false; });
            };
            ql.apDung = function (form) { if (form && form.$invalid) return; ql.trangSo = 1; ql.tai(); };
            ql.boLoc = function () { ql.loc = { sapXepTheo: 'NgayGiaoDich', chieuSapXep: 'DESC' }; ql.timKiem = ''; ql.apDung(); };
            ql.doiTrang = function (buoc) { var trangMoi = ql.trangSo + buoc; if (ql.dangTai || trangMoi < 1 || (trangMoi - 1) * ql.kichThuoc >= ql.tong) return; ql.trangSo = trangMoi; ql.tai(); };
            ql.chiDoc = function (item) { return ql.cauHinh.chiDoc || (ql.trang === 'danh-muc' && (item.laDanhMucHeThong || item.maNguoiDung == null)); };
            ql.tenItem = function (item) { return item.tenVi || item.tenDanhMuc || item.noiDung || item.tieuDe || item.hoTen || ('#' + item[ql.cauHinh.id]); };
            ql.laTaiKhoanHienTai = function (item) { return ql.trang === 'nguoi-dung' && phien.lay() && item.maNguoiDung === phien.lay().maNguoiDung; };
            ql.luaChon = function (field) {
                if (!ql.duLieu) return [];
                if (!field.nguon) return field.luaChon;
                var loaiCanChon = ql.trang === 'ngan-sach' ? 'CHI' : ql.duLieu.loaiGiaoDich;
                var khoa = field.nguon === 'vi' ? 'vi:' + ql.sua + ':' + ql.duLieu.maVi : 'dm:' + loaiCanChon;
                var nguon = field.nguon === 'vi' ? ql.vi : ql.danhMuc;
                // Giữ cùng đối tượng option giữa các lần digest để ngOptions không tạo vòng lặp cập nhật.
                if (!luaChonCache[khoa] || luaChonCache[khoa].nguon !== nguon) {
                    var options = field.nguon === 'vi'
                        ? nguon.filter(function (v) { return v.trangThai || (ql.sua && v.maVi === ql.duLieu.maVi); }).map(function (v) { return { ma: v.maVi, ten: v.tenVi + ' (#' + v.maVi + ')' }; })
                        : nguon.filter(function (d) { return d.loaiDanhMuc === loaiCanChon; }).map(function (d) { return { ma: d.maDanhMuc, ten: d.tenDanhMuc + ' (#' + d.maDanhMuc + ')' }; });
                    luaChonCache[khoa] = { nguon: nguon, options: options };
                }
                return luaChonCache[khoa].options;
            };
            ql.doiLoai = function (field) { if (field.ma === 'loaiGiaoDich') ql.duLieu.maDanhMuc = null; };
            ql.hienTruong = function (field) { return !!ql.duLieu && !(field.chiThem && ql.sua) && !(field.chiSua && !ql.sua) && !(field.khiLapLai && !ql.duLieu.lapLai); };
            function moModal() {
                phanTuTruocModal = document.activeElement;
                timeout(function () { var el = document.querySelector('.modal-backdrop input, .modal-backdrop button'); if (el) el.focus(); }, 0);
            }
            ql.mo = function (item) {
                if (ql.dangLuu || ql.dangTai || (item && ql.chiDoc(item))) return;
                ql.sua = !!item; ql.ma = item && item[ql.cauHinh.id]; ql.loiModal = null; ql.xungDot = false; ql.chuaXacDinh = false;
                ql.duLieu = item ? angular.copy(item) : { loaiGiaoDich: 'CHI', loaiDanhMuc: 'CHI', ngayGiaoDich: new Date(), ngayNhac: new Date(Date.now() + 3600000), thang: homNay.getMonth() + 1, nam: homNay.getFullYear(), phanTramCanhBao: 80, trangThai: true, lapLai: false };
                ql.cauHinh.truong.forEach(function (field) { if (field.kieu === 'datetime-local') ql.duLieu[field.ma] = new Date(ql.duLieu[field.ma]); if (field.kieu === 'tien' && ql.duLieu[field.ma] != null) ql.duLieu[field.ma] = String(ql.duLieu[field.ma]); });
                ql.moSua = true; moModal();
            };
            ql.dong = function () {
                if (ql.dangLuu) return;
                ql.moSua = false; ql.canXoa = null; ql.chiTiet = null; ql.duLieu = null; ql.loiModal = null;
                if (phanTuTruocModal && document.contains(phanTuTruocModal)) phanTuTruocModal.focus();
            };
            ql.luu = function (form) {
                if (form.$invalid || ql.dangLuu || ql.xungDot || ql.chuaXacDinh) return;
                var body = {};
                ql.cauHinh.truong.forEach(function (field) {
                    if (!ql.hienTruong(field)) return;
                    var v = ql.duLieu[field.ma];
                    if (field.kieu === 'datetime-local') v = ql.trang === 'nhac-nho' ? v.toISOString() : api.ngayLocal(v);
                    if (typeof v === 'string' && field.kieu !== 'password') v = v.trim();
                    body[field.ma] = v == null ? (field.kieu === 'checkbox' ? false : null) : v;
                });
                if (ql.trang === 'nhac-nho' && !body.lapLai) body.kieuLapLai = null;
                if ((body.soTien != null && Number(body.soTien) <= 0) || (body.soTienGioiHan != null && Number(body.soTienGioiHan) <= 0)) { ql.loiModal = 'Số tiền phải lớn hơn 0.'; return; }
                if (ql.sua && ['giao-dich', 'nhac-nho'].indexOf(ql.trang) >= 0) body.phienBan = ql.duLieu.phienBan;
                ql.dangLuu = true; ql.loiModal = null;
                var request = ql.sua ? api.put(ql.cauHinh.host || 'user', ql.cauHinh.api + '/' + ql.ma, body) : api.post(ql.cauHinh.host || 'user', ql.cauHinh.api, body);
                request.then(function (d) {
                    if (daDong) return;
                    if (ql.trang === 'nguoi-dung' && phien.lay() && ql.ma === phien.lay().maNguoiDung && body.trangThai === false) { phien.xoa(); location.path('/dang-nhap'); return; }
                    ql.dangLuu = false; ql.dong(); thongBao(d.thongBao || 'Đã lưu thành công.'); ql.tai();
                    if (d.canhBaoNganSach && d.canhBaoNganSach.length) root.$broadcast('canhBaoNganSach', d.canhBaoNganSach);
                    if (d.kiemTraCanhBaoThanhCong === false) thongBao('Giao dịch đã lưu. Chưa kiểm tra được cảnh báo; hãy mở Ngân sách để xem mức chi hiện tại.', 'loi');
                }).catch(function (e) { if (!daDong) { ql.loiModal = api.loi(e); ql.xungDot = e.status === 409 && ['giao-dich', 'nhac-nho'].indexOf(ql.trang) >= 0;
                    ql.chuaXacDinh = !ql.sua && (e.status === 0 || e.status === -1 || e.status >= 500);
                    if (ql.chuaXacDinh) ql.loiModal = 'Chưa xác định được bản ghi đã tạo hay chưa. Hãy đóng biểu mẫu và tải lại danh sách trước khi thêm lại, để tránh tạo trùng.';
                } })
                    .finally(function () { ql.dangLuu = false; });
            };
            ql.taiLaiBanSua = function () { if (ql.dangLuu) return; var id = ql.ma; ql.dangLuu = true; api.get(ql.cauHinh.host || 'user', ql.cauHinh.api + '/' + id).then(function (d) { if (!daDong) { ql.dangLuu = false; ql.mo(d); } }).catch(function (e) { ql.loiModal = api.loi(e); }).finally(function () { ql.dangLuu = false; }); };
            ql.hoiXoa = function (item) { if (!ql.chiDoc(item) && !ql.dangLuu) { ql.canXoa = item; ql.loiModal = null; moModal(); } };
            ql.xoa = function () {
                if (ql.dangLuu || !ql.canXoa) return; var xoaTaiKhoanHienTai = ql.laTaiKhoanHienTai(ql.canXoa); ql.dangLuu = true;
                api.xoa(ql.cauHinh.host || 'user', ql.cauHinh.api + '/' + ql.canXoa[ql.cauHinh.id]).then(function (d) {
                    if (daDong) return; ql.dangLuu = false; ql.dong();
                    if (xoaTaiKhoanHienTai) { phien.xoa(); location.path('/dang-nhap'); return; }
                    thongBao(d.thongBao || 'Đã xóa.'); if (ql.danhSach.length === 1 && ql.trangSo > 1) ql.trangSo--; ql.tai();
                }).catch(function (e) { if (!daDong) ql.loiModal = api.loi(e); }).finally(function () { ql.dangLuu = false; });
            };
            ql.xemAudit = function (item) { ql.dangLuu = true; api.get('admin', 'NhatKyAudit/' + item.maAudit).then(function (d) {
                if (daDong) return; ['duLieuCu', 'duLieuMoi'].forEach(function (key) { try { d[key] = JSON.stringify(JSON.parse(d[key]), null, 2); } catch (_) { /* Giữ nguyên dữ liệu dạng văn bản. */ } }); ql.chiTiet = d; moModal();
            }).catch(loi).finally(function () { ql.dangLuu = false; }); };
            function datLichPDF() {
                interval.cancel(lichPDF);
                if (!ql.danhSach.some(function (b) { return b.trangThai === 'CHO_XU_LY' || b.trangThai === 'DANG_XU_LY'; })) return;
                var dangQuet = false;
                lichPDF = interval(function () {
                    if (document.hidden || dangQuet || ql.dangTai || !phien.lay()) return;
                    dangQuet = true; var lan = lanTai;
                    api.get('user', 'BaoCaoThang', { trang: ql.trangSo, kichThuocTrang: ql.kichThuoc }).then(function (d) { if (!daDong && lan === lanTai) xuLyDanhSach(d); }).catch(function (e) { if (lan === lanTai) { interval.cancel(lichPDF); loi(e); } }).finally(function () { dangQuet = false; });
                }, 3000);
            }
            ql.taoPDF = function (form) {
                if (form.$invalid || ql.dangPDF) return;
                var khoa = ql.baoCao.thang + '-' + ql.baoCao.nam;
                if (khoa !== khoaPDF || !maPDF) { khoaPDF = khoa; maPDF = api.maYeuCau(); }
                ql.dangPDF = true; ql.loi = null;
                api.post('user', 'BaoCaoThang', { thang: ql.baoCao.thang, nam: ql.baoCao.nam, maYeuCau: maPDF }).then(function (d) {
                    if (daDong) return; maPDF = null; thongBao(d.thongBao); ql.trangSo = 1; ql.tai();
                }).catch(loi).finally(function () { ql.dangPDF = false; });
            };
            ql.taiPDF = function (item) { if (ql.dangFile || item.trangThai !== 'HOAN_THANH') return; ql.dangFile = true; api.tai('user', 'BaoCaoThang/' + item.maBaoCao + '/tai-file', null, 'QuanLyNganQuyCaNhan-BaoCao-' + item.thang + '-' + item.nam + '.pdf').catch(loi).finally(function () { ql.dangFile = false; }); };
            ql.xuatFile = function (mau, form) {
                if (ql.dangFile || (form && form.$invalid)) return;
                var params = { dinhDang: ql.xuat.dinhDang };
                if (!mau) ['tuNgay', 'denNgay', 'maVi', 'maDanhMuc'].forEach(function (key) { var v = ql.xuat[key]; if (v != null && v !== '') params[key] = v instanceof Date ? api.ngayLocal(v) : v; });
                ql.dangFile = true; ql.loi = null;
                api.tai('user', mau ? 'GiaoDich/mau-file' : 'GiaoDich/export', params, 'QuanLyNganQuyCaNhan-' + (mau ? 'MauGiaoDich' : 'GiaoDich') + '.' + ql.xuat.dinhDang).catch(loi).finally(function () { ql.dangFile = false; });
            };
            ql.chonFile = function (file) { ql.fileNhap = file; ql.maNhap = file ? api.maYeuCau() : null; ql.ketQuaNhap = null; ql.loiNhap = null; };
            ql.nhapFile = function () {
                if (ql.dangNhapFile || !ql.fileNhap) return;
                if (!/\.(csv|xlsx)$/i.test(ql.fileNhap.name) || !ql.fileNhap.size || ql.fileNhap.size > 5 * 1024 * 1024) { ql.loiNhap = 'Chọn file CSV hoặc XLSX có dữ liệu, tối đa 5 MB.'; return; }
                var data = new FormData(); data.append('File', ql.fileNhap); data.append('MaYeuCau', ql.maNhap);
                ql.dangNhapFile = true; ql.loiNhap = null;
                api.post('user', 'GiaoDich/import', data, { transformRequest: angular.identity, headers: { 'Content-Type': undefined } }).then(function (d) {
                    if (daDong) return; ql.ketQuaNhap = d; thongBao(d.thongBao); // Giữ mã yêu cầu khi bấm lại để backend không nhập trùng.
                }).catch(function (e) { if (!daDong) ql.loiNhap = api.loi(e); }).finally(function () { ql.dangNhapFile = false; });
            };
            ql.taiThongBao = function () {
                var lan = ++lanThongBao; ql.dangTaiThongBao = true; ql.loiThongBao = null;
                api.get('user', 'ThongBaoNhacNho', { trang: ql.trangThongBao, kichThuocTrang: 10, chiChuaDoc: false }).then(function (d) { if (!daDong && lan === lanThongBao) { ql.thongBao = d.danhSach; ql.tongThongBao = d.tongSoBanGhi; } }).catch(function (e) { if (!daDong && lan === lanThongBao) ql.loiThongBao = api.loi(e); }).finally(function () { if (lan === lanThongBao) ql.dangTaiThongBao = false; });
            };
            ql.doiTrangThongBao = function (buoc) { var trangMoi = ql.trangThongBao + buoc; if (trangMoi < 1 || (trangMoi - 1) * 10 >= ql.tongThongBao || ql.dangTaiThongBao) return; ql.trangThongBao = trangMoi; ql.taiThongBao(); };
            ql.docThongBao = function (item) { if (ql.dangDoc) return; ql.dangDoc = true; api.put('user', 'ThongBaoNhacNho/' + item.maThongBao + '/da-doc').then(function () { if (!daDong) root.$broadcast('thongBaoDaDoc'); }).catch(function (e) { ql.loiThongBao = api.loi(e); }).finally(function () { ql.dangDoc = false; }); };
            function phimModal(e) {
                if (!ql.moSua && !ql.canXoa && !ql.chiTiet) return;
                if (e.key === 'Escape') { scope.$evalAsync(ql.dong); return; }
                if (e.key !== 'Tab') return;
                var panel = document.querySelector('.management-modal'), nodes = panel && panel.querySelectorAll('button:not(:disabled), input:not(:disabled), select:not(:disabled), textarea:not(:disabled), a[href]');
                if (!nodes || !nodes.length) return;
                if (e.shiftKey && document.activeElement === nodes[0]) { nodes[nodes.length - 1].focus(); e.preventDefault(); }
                else if (!e.shiftKey && document.activeElement === nodes[nodes.length - 1]) { nodes[0].focus(); e.preventDefault(); }
            }
            document.addEventListener('keydown', phimModal);
            scope.$on('locNoiDung', function (_, text) { ql.timKiem = text; });
            scope.$on('thongBaoDaDoc', function () { if (ql.trang === 'nhac-nho') ql.taiThongBao(); });
            scope.$on('$destroy', function () { daDong = true; interval.cancel(lichPDF); document.removeEventListener('keydown', phimModal); ql.duLieu = null; });
            function taiLuaChon() {
                ql.dangTai = true; ql.loi = null;
                return q.all([api.get('user', 'Vi'), api.get('user', 'DanhMuc')]).then(function (d) { if (!daDong) { ql.vi = d[0]; ql.danhMuc = d[1]; ql.loiLuaChon = false; return ql.tai(); } }).catch(function (e) { loi(e); ql.dangTai = false; ql.loiLuaChon = true; });
            }
            if (['giao-dich', 'ngan-sach', 'bao-cao'].indexOf(ql.trang) >= 0) taiLuaChon(); else ql.tai();
        }]);
})();
