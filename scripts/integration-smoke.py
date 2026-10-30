#!/usr/bin/env python3
"""Exercise the complete local repair workflow; generated development records remain.

Supply the existing administrator through SMOKE_ADMIN_USERNAME and
SMOKE_ADMIN_PASSWORD. Credentials, tokens and response bodies are never printed.
"""
import argparse
import json
import os
import secrets
import sys
import urllib.error
import urllib.parse
import urllib.request
from decimal import Decimal


class SmokeFailure(Exception):
    pass


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-url', default='http://127.0.0.1:5107')
    args = parser.parse_args()
    origin = urllib.parse.urlsplit(args.base_url)
    if (origin.scheme != 'http' or origin.hostname not in {'localhost', '127.0.0.1', '::1'}
            or origin.username or origin.password or origin.query or origin.fragment or origin.path not in {'', '/'}):
        raise SmokeFailure('Base URL must be a local HTTP origin.')
    admin_name = os.environ.get('SMOKE_ADMIN_USERNAME')
    admin_password = os.environ.get('SMOKE_ADMIN_PASSWORD')
    if not admin_name or not admin_password:
        raise SmokeFailure('Set SMOKE_ADMIN_USERNAME and SMOKE_ADMIN_PASSWORD in the process environment.')
    base = args.base_url.rstrip('/')
    completed = 0

    class NoRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, *unused):
            return None

    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}), NoRedirect())

    def check(condition, message):
        nonlocal completed
        if not condition:
            raise SmokeFailure(message)
        completed += 1
        print('PASS: ' + message, flush=True)

    def request(method, path, token=None, body=None, expected=200):
        headers = {'Accept': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer ' + token
        if body is not None:
            headers['Content-Type'] = 'application/json'
        req = urllib.request.Request(base + path, method=method, headers=headers,
                                     data=None if body is None else json.dumps(body).encode())
        try:
            response = opener.open(req, timeout=60)
        except urllib.error.HTTPError as error:
            response = error
        except (OSError, TimeoutError):
            raise SmokeFailure(method + ' ' + path + ': connection failed') from None
        with response:
            status, payload = response.code, response.read()
        if status != expected:
            raise SmokeFailure(method + ' ' + path + ': expected HTTP ' + str(expected) + ', got ' + str(status))
        if expected >= 400:
            return None
        try:
            result = json.loads(payload)
        except (ValueError, UnicodeDecodeError):
            raise SmokeFailure(method + ' ' + path + ': invalid JSON') from None
        if not isinstance(result, dict) or result.get('success') is not True:
            raise SmokeFailure(method + ' ' + path + ': operation did not report success')
        return result.get('data')

    def login(name, password):
        result = request('POST', '/api/auth/login', body={'username': name, 'password': password})
        if not isinstance(result, dict) or not result.get('success') or not result.get('token', {}).get('accessToken'):
            raise SmokeFailure('Login omitted access token.')
        return result['token']

    suffix = secrets.token_hex(6)
    phone = lambda: '09' + ''.join(str(secrets.randbelow(10)) for _ in range(9))
    admin = login(admin_name, admin_password)['accessToken']
    check(request('GET', '/api/auth/me', admin)['vaiTro'] == 'ADMIN', 'Administrator authenticates')
    employees = {}
    for role in ('LE_TAN', 'THU_KHO', 'KY_THUAT_VIEN'):
        name, password = role.lower() + '_' + suffix, secrets.token_urlsafe(24)
        employee = request('POST', '/api/nhan-vien', admin, {
            'hoTen': 'Smoke ' + role, 'soDienThoai': phone(), 'chucVu': role,
            'tenDangNhap': name, 'matKhau': password, 'chuyenMon': 'Laptop' if role == 'KY_THUAT_VIEN' else None
        }, expected=201)
        token = login(name, password)['accessToken']
        check(request('GET', '/api/auth/me', token)['vaiTro'] == role, role + ' employee is provisioned and authenticates')
        employees[role] = (employee['id'], token)
    keeper_id, keeper = employees['THU_KHO']
    tech_id, tech = employees['KY_THUAT_VIEN']
    reception = employees['LE_TAN'][1]

    customers = []
    for number in (1, 2):
        name, password = 'customer_' + str(number) + '_' + suffix, secrets.token_urlsafe(24)
        request('POST', '/api/auth/register', body={'username': name, 'password': password,
                'confirmPassword': password, 'hoTen': 'Smoke customer', 'soDienThoai': phone()})
        tokens = login(name, password)
        profiles = request('GET', '/api/khach-hang', tokens['accessToken'])['items']
        check(len(profiles) == 1, 'Customer ' + str(number) + ' sees only their own profile')
        customers.append((profiles[0]['id'], tokens))
    customer_id, customer_tokens = customers[0]
    customer, other_customer = customer_tokens['accessToken'], customers[1][1]['accessToken']

    supplier = request('POST', '/api/nha-cung-cap', admin, {'tenNcc': 'Smoke supplier ' + suffix}, expected=201)
    part = request('POST', '/api/linh-kien', admin, {'tenLinhKien': 'Smoke part ' + suffix,
        'donViTinh': 'cai', 'donGiaBan': 150000, 'thoiHanBaoHanhThang': 6}, expected=201)
    service = request('POST', '/api/dich-vu', admin, {'tenDichVu': 'Smoke service ' + suffix, 'giaNiemYet': 100000}, expected=201)
    images_path = '/api/linh-kien/' + str(part['id']) + '/anh'
    first_image = request('POST', images_path, admin, {'duongDan': 'https://images.example/first.png'}, expected=201)
    second_image = request('POST', images_path, keeper, {'duongDan': 'https://images.example/second.png'}, expected=201)
    request('PUT', images_path + '/' + str(second_image['id']) + '/chinh', admin)
    images = request('GET', images_path)
    check(len(images) == 2 and sum(image['laAnhChinh'] for image in images) == 1
          and next(image['id'] for image in images if image['laAnhChinh']) == second_image['id'], 'Multiple part images retain exactly one primary image')
    request('DELETE', images_path + '/' + str(second_image['id']), keeper)
    check(request('GET', images_path)[0]['id'] == first_image['id'] and request('GET', images_path)[0]['laAnhChinh'], 'Deleting the primary image promotes the remaining image')
    warehouse = request('POST', '/api/kho', admin, {'tenKho': 'Smoke warehouse ' + suffix, 'thuKhoId': keeper_id}, expected=201)
    receipt = request('POST', '/api/phieu-nhap', keeper, {'nhaCungCapId': supplier['id'], 'khoId': warehouse['id'],
        'chiTiet': [{'linhKienId': part['id'], 'soLuongNhap': 5, 'donGiaNhap': 75000}]}, expected=201)
    receipt_path = '/api/phieu-nhap/' + str(receipt['id']) + '/xac-nhan'
    check(request('POST', receipt_path, keeper)['trangThai'] == 'DA_NHAP_KHO', 'Receipt confirmation imports stock')
    request('POST', receipt_path, keeper, expected=409)
    check(True, 'Duplicate receipt confirmation is rejected')

    def stock():
        rows = request('GET', '/api/ton-kho?khoId=' + str(warehouse['id']), keeper)
        return next(row['soLuongTon'] for row in rows if row['linhKienId'] == part['id'])

    check(stock() == 5, 'Receipt adds exactly five parts')
    device = request('POST', '/api/thiet-bi', customer, {'khachHangId': customer_id,
        'tenThietBi': 'Smoke laptop', 'soSerial': 'SMOKE-' + suffix}, expected=201)
    device_body = {'khachHangId': customer_id, 'tenThietBi': 'Smoke laptop', 'soSerial': 'SMOKE-' + suffix}
    request('PUT', '/api/thiet-bi/' + str(device['id']), keeper, device_body, expected=403)
    request('PUT', '/api/khach-hang/' + str(customer_id), tech,
            {'hoTen': 'Unpermitted profile edit', 'soDienThoai': phone()}, expected=403)
    check(request('PUT', '/api/thiet-bi/' + str(device['id']), customer, device_body)['id'] == device['id'], 'Only the owner or authorized reception staff can edit customer devices')
    request('GET', '/api/thiet-bi/' + str(device['id']), other_customer, expected=404)
    check(True, 'Another customer cannot read the device')
    repair = request('POST', '/api/phieu-sua-chua', reception,
                     {'thietBiId': device['id'], 'tinhTrangLoi': 'Smoke broken display'}, expected=201)
    repair_id = repair['phieu']['id']
    repair_path = '/api/phieu-sua-chua/' + str(repair_id)
    request('GET', repair_path, other_customer, expected=404)
    check(True, 'Another customer cannot read the repair')
    request('PUT', repair_path + '/phan-cong', reception, {'kyThuatVienId': tech_id})
    check(request('PUT', repair_path + '/trang-thai', tech, {'trangThai': 'DANG_SUA'})['phieu']['trangThai'] == 'DANG_SUA',
          'Assigned technician starts repair')
    request('DELETE', '/api/thiet-bi/' + str(device['id']), customer, expected=409)
    check(True, 'A device with an unfinished repair cannot be deleted')
    service_path = repair_path + '/dich-vu/' + str(service['id'])
    request('PUT', service_path, tech, {'soLuong': 1})
    dispatch = {'phieuId': repair_id, 'khoId': warehouse['id'], 'linhKienId': part['id'], 'soLuong': 6}
    request('POST', '/api/xuat-kho', keeper, dispatch, expected=409)
    check(stock() == 5, 'Insufficient-stock dispatch is rejected without changing stock')
    dispatch['soLuong'] = 2
    request('POST', '/api/xuat-kho', keeper, dispatch, expected=201)
    check(stock() == 3, 'Successful dispatch deducts exactly two parts')
    check(Decimal(str(request('GET', repair_path, customer)['tongChiPhi'])) == 400000, 'Repair sums service and part price snapshots')
    request('PUT', repair_path + '/trang-thai', tech, {'trangThai': 'HOAN_THANH', 'ketQuaSuaChua': 'Replaced display and tested'})
    request('PUT', service_path, tech, {'soLuong': 2}, expected=409)
    check(True, 'Completed repair costs are immutable')
    request('PUT', repair_path + '/trang-thai', reception, {'trangThai': 'DA_TRA'}, expected=409)
    check(True, 'Device cannot be returned before payment')

    invoice_body = {'phieuId': repair_id, 'giamGia': 10000}
    invoice = request('POST', '/api/hoa-don', reception, invoice_body, expected=201)
    request('POST', '/api/hoa-don', reception, invoice_body, expected=409)
    check(True, 'Duplicate active invoice is rejected')
    invoice_path = '/api/hoa-don/' + str(invoice['id'])
    check(request('POST', invoice_path + '/huy', reception, {'lyDo': 'Smoke replacement check'})['trangThai'] == 'HUY', 'Unpaid invoice can be canceled')
    invoice = request('POST', '/api/hoa-don', reception, invoice_body, expected=201)
    invoice_path = '/api/hoa-don/' + str(invoice['id'])
    check(Decimal(str(invoice['tongThanhToan'])) == 390000, 'Replacement invoice applies the correct discount')
    request('GET', invoice_path, other_customer, expected=404)
    check(request('GET', invoice_path, customer)['id'] == invoice['id'], 'Invoice ownership is enforced')
    check(request('POST', invoice_path + '/thanh-toan', reception, {'phuongThucTt': 'TIEN_MAT'})['trangThai'] == 'DA_THANH_TOAN', 'Invoice payment succeeds')
    request('POST', invoice_path + '/huy', reception, {'lyDo': 'Invalid paid cancellation'}, expected=409)
    check(True, 'Paid invoice cannot be canceled')
    returned = request('PUT', repair_path + '/trang-thai', reception, {'trangThai': 'DA_TRA'})
    check(returned['phieu']['trangThai'] == 'DA_TRA' and returned['phieu']['ngayTra'] is not None, 'Paid device is returned and timestamped')
    history = request('GET', repair_path, customer)['lichSu']
    check({'TIEP_NHAN', 'DANG_SUA', 'HOAN_THANH', 'DA_TRA'}.issubset({event['trangThaiMoi'] for event in history}), 'Repair history records the complete workflow')
    movements = request('GET', '/api/ton-kho/bien-dong?khoId=' + str(warehouse['id']), keeper)
    check(len(movements) == 2 and stock() == 3, 'Stock movement audit reconciles receipt and dispatch')
    account_id = request('GET', '/api/auth/me', customer)['taiKhoanId']
    request('DELETE', '/api/thiet-bi/' + str(device['id']), customer)
    request('GET', '/api/thiet-bi/' + str(device['id']), customer, expected=404)
    request('DELETE', '/api/khach-hang/' + str(customer_id), reception)
    request('GET', '/api/auth/me', customer, expected=401)
    request('PUT', '/api/users/' + str(account_id) + '/status', admin, {'isActive': True}, expected=409)
    check(True, 'Completed device deletion and customer deletion revoke access without allowing profile bypass')
    print('Integration smoke passed: ' + str(completed) + ' assertions. Generated local development records remain.', flush=True)


if __name__ == '__main__':
    try:
        main()
    except SmokeFailure as error:
        print('FAIL: ' + str(error), file=sys.stderr)
        sys.exit(1)
    except Exception as error:
        print('FAIL: unexpected ' + type(error).__name__ + ' (details suppressed)', file=sys.stderr)
        sys.exit(1)
