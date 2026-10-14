(function () {
    'use strict';
    angular.module('QuanLyNganQuyCaNhan', ['ngRoute'])
        .constant('CauHinhAPI', window.CauHinhQuanLyNganQuyCaNhan)
        .config(['$routeProvider', '$locationProvider', '$httpProvider', function (routes, location, http) {
            location.hashPrefix('!');
            ['dang-nhap', 'dang-ky', 'quen-mat-khau', 'xac-thuc-2fa'].forEach(function (trang) {
                routes.when('/' + trang, { templateUrl: 'GiaoDien/QuanLyNganQuyCaNhan.XacThuc.html', controller: 'XacThucController', controllerAs: 'xt', maTrang: trang });
            });
            routes.when('/tong-quan', { templateUrl: 'GiaoDien/QuanLyNganQuyCaNhan.TongQuan.html', controller: 'TongQuanController', controllerAs: 'tq', baoVe: true, maTrang: 'tong-quan' });
            ['giao-dich', 'vi', 'danh-muc', 'ngan-sach', 'nhac-nho', 'bao-cao', 'nguoi-dung', 'danh-muc-he-thong', 'nhat-ky', 'suc-khoe'].forEach(function (trang) {
                routes.when('/' + trang, { templateUrl: 'GiaoDien/QuanLyNganQuyCaNhan.QuanLy.html', controller: 'QuanLyController', controllerAs: 'ql', baoVe: true,
                    admin: ['nguoi-dung', 'danh-muc-he-thong', 'nhat-ky', 'suc-khoe'].indexOf(trang) >= 0, maTrang: trang });
            });
            routes.when('/bao-mat', { templateUrl: 'GiaoDien/QuanLyNganQuyCaNhan.BaoMat.html', controller: 'BaoMatController', controllerAs: 'bm', baoVe: true, maTrang: 'bao-mat' });
            routes.otherwise({ redirectTo: '/tong-quan' });
            http.interceptors.push('LamMoiTokenInterceptor');
        }])
        .filter('tienViet', function () {
            var dinhDang = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 });
            return function (giaTri) { if (giaTri == null || giaTri === '' || !Number.isFinite(Number(giaTri))) return '—'; return dinhDang.format(Number(giaTri)) + ' ₫'; };
        })
        .filter('trangThaiViet', function () {
            var ten = { CHO_XU_LY: 'Đang chờ', DANG_XU_LY: 'Đang tạo PDF', HOAN_THANH: 'Hoàn thành', LOI: 'Có lỗi', HANG_NGAY: 'Hằng ngày', HANG_TUAN: 'Hằng tuần', HANG_THANG: 'Hằng tháng' };
            return function (ma) { return ten[ma] || ma; };
        })
        .run(['$rootScope', '$location', 'PhienDangNhap', function (root, location, phien) {
            root.$on('$routeChangeStart', function (event, next) {
                var route = next && next.$$route;
                if (!route) return;
                if (route.baoVe && !phien.lay()) { event.preventDefault(); location.path('/dang-nhap'); }
                else if (route.admin && phien.lay().vaiTro !== 'Admin') { event.preventDefault(); location.path('/tong-quan'); }
                else if (!route.baoVe && phien.lay()) { event.preventDefault(); location.path('/tong-quan'); }
            });
        }]);
})();
