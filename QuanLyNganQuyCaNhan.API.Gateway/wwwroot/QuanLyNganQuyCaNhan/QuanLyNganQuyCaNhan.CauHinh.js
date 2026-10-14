/* Frontend và toàn bộ API dùng chung địa chỉ của Ocelot Gateway.
   Đổi downstream trong QuanLyNganQuyCaNhan.Ocelot.json khi đổi cổng backend. */
window.CauHinhQuanLyNganQuyCaNhan = {
    apiDungChung: window.location.origin + '/api',
    apiNguoiDung: window.location.origin + '/api',
    apiAdmin: window.location.origin + '/api'
};
