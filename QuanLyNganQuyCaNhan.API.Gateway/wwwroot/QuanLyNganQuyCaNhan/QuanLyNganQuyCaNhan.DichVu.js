(function () {
    'use strict';
    var ungDung = angular.module('QuanLyNganQuyCaNhan');
    ungDung.factory('PhienDangNhap', ['$rootScope', function (root) {
        var khoa = 'QuanLyNganQuyCaNhan.Phien', phien = null, phienBan = 0;
        try { phien = JSON.parse(sessionStorage.getItem(khoa)); } catch (_) { phien = null; }
        if (!phien || !phien.accessToken || !phien.refreshToken) phien = null;
        function ghi() { try { if (phien) sessionStorage.setItem(khoa, JSON.stringify(phien)); else sessionStorage.removeItem(khoa); } catch (_) { /* Phiên vẫn hoạt động trong bộ nhớ. */ } }
        return {
            lay: function () { return phien; },
            phienBan: function () { return phienBan; },
            luu: function (duLieu) { phien = { maNguoiDung: duLieu.maNguoiDung, accessToken: duLieu.accessToken, refreshToken: duLieu.refreshToken, hoTen: duLieu.hoTen, tenDangNhap: duLieu.tenDangNhap, vaiTro: duLieu.vaiTro }; phienBan++; ghi(); root.$broadcast('phienThayDoi'); },
            doiToken: function (token) { if (phien) { phien.accessToken = token; ghi(); } },
            xoa: function () { phien = null; phienBan++; ghi(); root.$broadcast('phienThayDoi'); }
        };
    }]);
    ungDung.factory('LamMoiTokenInterceptor', ['$q', '$injector', 'PhienDangNhap', 'CauHinhAPI', function (q, injector, phien, config) {
        var dangLamMoi = null;
        function dungAPI(url) { return [config.apiDungChung, config.apiNguoiDung, config.apiAdmin].some(function (base) { return url.indexOf(base + '/') === 0; }); }
        return {
            request: function (request) {
                var dangNhap = phien.lay();
                if (request.canToken && dungAPI(request.url) && dangNhap) request.headers.Authorization = 'Bearer ' + dangNhap.accessToken;
                return request;
            },
            responseError: function (loi) {
                if (loi.status !== 401 || !loi.config.canToken || loi.config.daThuLai || !phien.lay()) return q.reject(loi);
                var http = injector.get('$http'), phienBan = phien.phienBan();
                if (!dangLamMoi) {
                    dangLamMoi = http.post(config.apiDungChung + '/DangNhap/refresh', { refreshToken: phien.lay().refreshToken }, { timeout: 45000 }).then(function (response) {
                        if (phienBan !== phien.phienBan() || !phien.lay()) return q.reject(loi);
                        phien.doiToken(response.data.accessToken);
                    }).catch(function (err) {
                        if (phienBan === phien.phienBan()) { phien.xoa(); injector.get('$location').path('/dang-nhap'); }
                        return q.reject(err);
                    }).finally(function () { dangLamMoi = null; });
                }
                return dangLamMoi.then(function () {
                    if (phienBan !== phien.phienBan() || !phien.lay()) return q.reject(loi);
                    var request = angular.copy(loi.config); request.daThuLai = true;
                    return http(request);
                });
            }
        };
    }]);
    ungDung.factory('NganQuyAPI', ['$http', '$q', 'CauHinhAPI', function (http, q, config) {
        var bases = { chung: config.apiDungChung, user: config.apiNguoiDung, admin: config.apiAdmin };
        function gui(host, method, path, data, params, extra) {
            return http(angular.extend({ url: bases[host] + '/' + path, method: method, data: data, params: params, canToken: true, timeout: 45000 }, extra || {})).then(function (response) { return response.data; });
        }
        function tai(host, path, params, tenFile) {
            return gui(host, 'GET', path, null, params, { responseType: 'blob' }).then(function (blob) {
                var url = URL.createObjectURL(blob), link = document.createElement('a');
                link.href = url; link.download = tenFile; document.body.appendChild(link); link.click(); link.remove();
                setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
            }).catch(function (err) {
                if (err.data instanceof Blob) return q.when(err.data.text()).then(function (text) { try { err.data = JSON.parse(text); } catch (_) { err.data = { thongBao: 'Không tải được file.' }; } return q.reject(err); });
                return q.reject(err);
            });
        }
        return { get: function (h, p, params) { return gui(h, 'GET', p, null, params); }, post: function (h, p, d, extra) { return gui(h, 'POST', p, d, null, extra); },
            put: function (h, p, d) { return gui(h, 'PUT', p, d); }, xoa: function (h, p) { return gui(h, 'DELETE', p); }, tai: tai,
            congKhai: function (h, p, d) { return gui(h, 'POST', p, d, null, { canToken: false }); },
            anh: function (h, p) { return gui(h, 'GET', p, null, null, { responseType: 'blob' }); },
            loi: function (err) { var data = err && err.data; if (data && data.thongBao) return data.thongBao;
                if (data && data.errors) return Object.keys(data.errors).map(function (key) { return data.errors[key].join(' '); }).join(' ');
                if (err && err.status === 409) return 'Dữ liệu đã thay đổi. Hãy tải lại trước khi sửa tiếp.';
                if (err && err.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
                if (err && err.status === 401) return 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
                if (err && err.status === 429) return 'Bạn thao tác quá nhanh. Vui lòng chờ một lát.';
                if (err && (err.status === 502 || err.status === 504)) return 'Gateway chưa nhận được phản hồi từ API. Kiểm tra API phía sau đang chạy đúng cổng hoặc chờ rồi tải lại.';
                if (err && (err.status === 0 || err.status === -1)) return 'Không kết nối được máy chủ. Kiểm tra các API đang chạy và kết nối mạng.';
                return (data && data.title) || 'Chưa thể thực hiện thao tác. Vui lòng thử lại.'; },
            maYeuCau: function () { var a = new Uint8Array(16); window.crypto.getRandomValues(a); a[6] = (a[6] & 15) | 64; a[8] = (a[8] & 63) | 128;
                var s = Array.from(a).map(function (n) { return n.toString(16).padStart(2, '0'); }).join(''); return s.slice(0, 8) + '-' + s.slice(8, 12) + '-' + s.slice(12, 16) + '-' + s.slice(16, 20) + '-' + s.slice(20); },
            ngayLocal: function (date) { var d = new Date(date), pad = function (v) { return String(v).padStart(2, '0'); }; return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate()) + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds()); }
        };
    }]);
    ungDung.controller('UngDungController', ['$scope', '$rootScope', '$location', '$interval', '$timeout', 'PhienDangNhap', 'NganQuyAPI', function (scope, root, location, interval, timeout, phien, api) {
        var app = this, dongToast, truocCanhBao;
        app.phien = phien.lay(); app.thongBao = []; app.chuaDoc = 0;
        app.menu = [ { path: 'tong-quan', ten: 'Tổng quan', icon: 'home' }, { path: 'vi', ten: 'Ví tiền', icon: 'wallet' }, { path: 'giao-dich', ten: 'Sổ thu chi', icon: 'swap' },
            { path: 'danh-muc', ten: 'Danh mục', icon: 'grid' }, { path: 'ngan-sach', ten: 'Ngân sách', icon: 'target' }, { path: 'nhac-nho', ten: 'Nhắc nhở', icon: 'bell' },
            { path: 'bao-cao', ten: 'Báo cáo & xuất nhập', icon: 'chart' }, { path: 'bao-mat', ten: 'Bảo mật tài khoản', icon: 'shield' } ];
        app.menuAdmin = [{ path: 'nguoi-dung', ten: 'Người dùng', icon: 'users' }, { path: 'danh-muc-he-thong', ten: 'Danh mục hệ thống', icon: 'grid' },
            { path: 'nhat-ky', ten: 'Nhật ký hoạt động', icon: 'history' }, { path: 'suc-khoe', ten: 'Trạng thái hệ thống', icon: 'pulse' }];
        app.toast = function (message, loai) { app.tinNhan = { noiDung: message, loai: loai || 'thanh-cong' }; timeout.cancel(dongToast); dongToast = timeout(function () { app.tinNhan = null; }, 7000); };
        app.dangXuat = function () { if (root.$broadcast('truocDangXuat').defaultPrevented) return; var current = phien.lay(); phien.xoa(); location.path('/dang-nhap'); if (current) api.congKhai('chung', 'DangNhap/logout', { refreshToken: current.refreshToken }).catch(angular.noop); };
        app.docThongBao = function (item) { api.put('user', 'ThongBaoNhacNho/' + item.maThongBao + '/da-doc').then(function () { root.$broadcast('thongBaoDaDoc'); }).catch(function (e) { app.toast(api.loi(e), 'loi'); }); };
        function taiThongBao() { if (!phien.lay()) return; var lanPhien = phien.phienBan(); return api.get('user', 'ThongBaoNhacNho', { chiChuaDoc: true, trang: 1, kichThuocTrang: 8 }).then(function (d) { if (phien.lay() && phien.phienBan() === lanPhien) { app.thongBao = d.danhSach; app.chuaDoc = d.tongSoBanGhi; } }).catch(angular.noop); }
        scope.$on('phienThayDoi', function () { app.phien = phien.lay(); app.moThongBao = false; app.thongBao = []; app.chuaDoc = 0; app.canhBao = null; app.tinNhan = null; taiThongBao(); });
        scope.$on('thongBaoDaDoc', taiThongBao);
        scope.$on('$routeChangeSuccess', function (_, current) { app.trang = current.maTrang; app.moMenu = false; app.moThongBao = false; });
        scope.$on('thongBao', function (_, message, loai) { app.toast(message, loai); });
        scope.$on('canhBaoNganSach', function (_, danhSach) { app.canhBao = danhSach; truocCanhBao = document.activeElement; timeout(function () { var button = document.querySelector('.warning-modal button'); if (button) button.focus(); }, 0); });
        app.dongCanhBao = function () { app.canhBao = null; if (truocCanhBao && document.contains(truocCanhBao)) truocCanhBao.focus(); else document.getElementById('noi-dung').focus(); };
        function phimCanhBao(e) { if (!app.canhBao || !app.canhBao.length) return; if (e.key === 'Escape') scope.$evalAsync(app.dongCanhBao); if (e.key === 'Tab') { var button = document.querySelector('.warning-modal button'); if (button) { e.preventDefault(); button.focus(); } } }
        document.addEventListener('keydown', phimCanhBao);
        scope.$on('timGiaoDich', function (_, noiDung) { app.timKiem = noiDung; });
        app.tim = function () { location.path('/giao-dich'); root.$broadcast('locNoiDung', app.timKiem); };
        app.denNoiDung = function () { document.getElementById('noi-dung').focus(); };
        var lich = interval(function () { if (!document.hidden) taiThongBao(); }, 45000); taiThongBao();
        scope.$on('$destroy', function () { interval.cancel(lich); timeout.cancel(dongToast); document.removeEventListener('keydown', phimCanhBao); });
    }]);
})();
