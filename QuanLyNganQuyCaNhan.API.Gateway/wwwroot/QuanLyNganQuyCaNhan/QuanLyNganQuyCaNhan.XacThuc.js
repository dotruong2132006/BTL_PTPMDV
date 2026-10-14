(function () {
    'use strict';
    angular.module('QuanLyNganQuyCaNhan')
        .factory('BuocDangNhap2FA', function () { return { maNguoiDung: null, tenDangNhap: '' }; })
        .controller('XacThucController', ['$scope', '$route', '$location', 'NganQuyAPI', 'PhienDangNhap', 'BuocDangNhap2FA', function (scope, route, location, api, phien, buoc2fa) {
            var xt = this, daDong = false;
            xt.trang = route.current.maTrang; xt.duLieu = {}; xt.buoc2fa = buoc2fa;
            xt.tieuDe = { 'dang-nhap': 'Chào mừng bạn trở lại', 'dang-ky': 'Bắt đầu hành trình tài chính', 'quen-mat-khau': 'Khôi phục tài khoản', 'xac-thuc-2fa': 'Xác thực hai bước' }[xt.trang];
            if (xt.trang === 'xac-thuc-2fa' && !buoc2fa.maNguoiDung) location.path('/dang-nhap');
            function dangNhapThanhCong(d) { if (daDong) return; buoc2fa.maNguoiDung = null; phien.luu(d); xt.duLieu = {}; location.path('/tong-quan'); }
            xt.gui = function (form) {
                if (form.$invalid || xt.dangGui) return;
                if ((xt.trang === 'dang-ky' || xt.trang === 'quen-mat-khau') && xt.duLieu.xacNhanMatKhau !== (xt.duLieu.matKhau || xt.duLieu.matKhauMoi)) { xt.loi = 'Hai mật khẩu không khớp.'; return; }
                xt.dangGui = true; xt.loi = null;
                var request;
                if (xt.trang === 'dang-nhap') request = api.congKhai('chung', 'DangNhap', { tenDangNhap: xt.duLieu.tenDangNhap, matKhau: xt.duLieu.matKhau }).then(function (d) {
                    if (daDong) return;
                    if (d.yeuCau2FA) { buoc2fa.maNguoiDung = d.maNguoiDung; buoc2fa.tenDangNhap = xt.duLieu.tenDangNhap; xt.duLieu = {}; location.path('/xac-thuc-2fa'); }
                    else dangNhapThanhCong(d);
                });
                else if (xt.trang === 'xac-thuc-2fa') request = api.congKhai('chung', 'DangNhap/xacnhan-2fa', { maNguoiDung: buoc2fa.maNguoiDung, maOTP: xt.duLieu.maOTP }).then(dangNhapThanhCong);
                else if (xt.trang === 'dang-ky') request = api.congKhai('user', 'DangKy', { tenDangNhap: xt.duLieu.tenDangNhap, hoTen: xt.duLieu.hoTen, email: xt.duLieu.email, soDienThoai: xt.duLieu.soDienThoai || null, matKhau: xt.duLieu.matKhau }).then(function (d) {
                    if (daDong) return; scope.$emit('thongBao', d.thongBao + '. Bạn có thể đăng nhập ngay.'); xt.duLieu = {}; location.path('/dang-nhap');
                });
                else request = api.congKhai('chung', 'QuenMatKhau/dat-lai', { tenDangNhap: xt.duLieu.tenDangNhap, maKhoiPhuc: xt.duLieu.maKhoiPhuc, matKhauMoi: xt.duLieu.matKhauMoi, maOTP: xt.duLieu.maOTP || null }).then(function (d) {
                    if (daDong) return; scope.$emit('thongBao', d.thongBao); xt.duLieu = {}; location.path('/dang-nhap');
                });
                request.catch(function (e) { if (!daDong) xt.loi = api.loi(e); }).finally(function () { xt.dangGui = false; });
            };
            scope.$on('$destroy', function () { daDong = true; xt.duLieu = {}; });
        }]);
})();
