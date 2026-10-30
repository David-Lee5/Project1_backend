#!/usr/bin/env python3
"""Verify simultaneous stock/invoice writes against the running local SQL-backed API.

Set SMOKE_ADMIN_USERNAME and SMOKE_ADMIN_PASSWORD in the process environment.
Creates isolated local development records and leaves them for inspection.
Credentials, tokens and response bodies are never printed.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import json
import os
import secrets
import sys
import threading
import urllib.error
import urllib.parse
import urllib.request


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
    admin_name, admin_password = os.environ.get('SMOKE_ADMIN_USERNAME'), os.environ.get('SMOKE_ADMIN_PASSWORD')
    if not admin_name or not admin_password:
        raise SmokeFailure('Set SMOKE_ADMIN_USERNAME and SMOKE_ADMIN_PASSWORD in the process environment.')
    base, completed = args.base_url.rstrip('/'), 0

    class NoRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, *unused):
            return None

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
        # Separate openers avoid sharing mutable HTTP handlers between racing workers.
        opener = urllib.request.build_opener(urllib.request.ProxyHandler({}), NoRedirect())
        try:
            response = opener.open(req, timeout=60)
        except urllib.error.HTTPError as error:
            response = error
        except (OSError, TimeoutError):
            raise SmokeFailure(method + ' ' + path + ': connection failed') from None
        with response:
            status, payload = response.code, response.read()
        if expected is not None and status != expected:
            raise SmokeFailure(method + ' ' + path + ': expected HTTP ' + str(expected) + ', got ' + str(status))
        if status >= 400:
            return (status, None) if expected is None else None
        try:
            result = json.loads(payload)
        except (ValueError, UnicodeDecodeError):
            raise SmokeFailure(method + ' ' + path + ': invalid JSON') from None
        if not isinstance(result, dict) or result.get('success') is not True:
            raise SmokeFailure(method + ' ' + path + ': operation did not report success')
        return (status, result.get('data')) if expected is None else result.get('data')

    def login(name, password):
        result = request('POST', '/api/auth/login', body={'username': name, 'password': password})
        if not isinstance(result, dict) or not result.get('success') or not result.get('token', {}).get('accessToken'):
            raise SmokeFailure('Login omitted access token.')
        return result['token']['accessToken']

    def race(method, path, token, body, winning_status, label):
        barrier = threading.Barrier(2)

        def worker():
            barrier.wait(timeout=10)
            return request(method, path, token, body, expected=None)

        with ThreadPoolExecutor(max_workers=2) as executor:
            pending = [executor.submit(worker) for _ in range(2)]
            results = [future.result(timeout=75) for future in pending]
        statuses = sorted(status for status, _ in results)
        check(statuses == sorted([winning_status, 409]), label + ' allows exactly one write and rejects the contender')
        return next(data for status, data in results if status == winning_status)

    suffix = secrets.token_hex(6)
    phone = lambda: '09' + ''.join(str(secrets.randbelow(10)) for _ in range(9))
    admin = login(admin_name, admin_password)
    check(request('GET', '/api/auth/me', admin)['vaiTro'] == 'ADMIN', 'Administrator authenticates')
    staff = {}
    for role in ('LE_TAN', 'THU_KHO', 'KY_THUAT_VIEN'):
        name, password = role.lower() + '_race_' + suffix, secrets.token_urlsafe(24)
        person = request('POST', '/api/nhan-vien', admin, {'hoTen': 'Concurrency ' + role,
            'soDienThoai': phone(), 'chucVu': role, 'tenDangNhap': name, 'matKhau': password}, expected=201)
        staff[role] = person['id'], login(name, password)
    keeper_id, keeper = staff['THU_KHO']
    tech_id, tech = staff['KY_THUAT_VIEN']
    reception = staff['LE_TAN'][1]
    customer = request('POST', '/api/khach-hang', admin, {'hoTen': 'Concurrency customer', 'soDienThoai': phone()}, expected=201)
    device = request('POST', '/api/thiet-bi', admin, {'khachHangId': customer['id'],
        'tenThietBi': 'Concurrency laptop', 'soSerial': 'RACE-' + suffix}, expected=201)
    repair = request('POST', '/api/phieu-sua-chua', reception,
        {'thietBiId': device['id'], 'tinhTrangLoi': 'Concurrent repair smoke'}, expected=201)
    repair_id = repair['phieu']['id']
    repair_path = '/api/phieu-sua-chua/' + str(repair_id)
    request('PUT', repair_path + '/phan-cong', reception, {'kyThuatVienId': tech_id})
    request('PUT', repair_path + '/trang-thai', tech, {'trangThai': 'DANG_SUA'})
    supplier = request('POST', '/api/nha-cung-cap', admin, {'tenNcc': 'Concurrency supplier ' + suffix}, expected=201)
    part = request('POST', '/api/linh-kien', admin, {'tenLinhKien': 'Concurrency part ' + suffix,
        'donViTinh': 'cai', 'donGiaBan': 100, 'thoiHanBaoHanhThang': 0}, expected=201)
    warehouse = request('POST', '/api/kho', admin, {'tenKho': 'Concurrency warehouse ' + suffix, 'thuKhoId': keeper_id}, expected=201)
    receipt_body = {'nhaCungCapId': supplier['id'], 'khoId': warehouse['id'],
        'chiTiet': [{'linhKienId': part['id'], 'soLuongNhap': 5, 'donGiaNhap': 50}]}
    receipt = request('POST', '/api/phieu-nhap', keeper, receipt_body, expected=201)
    receipt_path = '/api/phieu-nhap/' + str(receipt['id'])

    def stock():
        rows = request('GET', '/api/ton-kho?khoId=' + str(warehouse['id']), keeper)
        return next(row['soLuongTon'] for row in rows if row['linhKienId'] == part['id'])

    confirmed = race('POST', receipt_path + '/xac-nhan', keeper, None, 200, 'Concurrent receipt confirmation')
    check(confirmed['trangThai'] == 'DA_NHAP_KHO' and stock() == 5, 'Racing confirmations add the five parts exactly once')
    request('PUT', receipt_path, keeper, receipt_body, expected=409)
    check(stock() == 5, 'A confirmed receipt rejects edits without changing stock')
    dispatched = race('POST', '/api/xuat-kho', keeper, {'phieuId': repair_id,
        'khoId': warehouse['id'], 'linhKienId': part['id'], 'soLuong': 5}, 201, 'Concurrent dispatch of all remaining stock')
    check(dispatched['soLuongXuat'] == 5 and stock() == 0, 'Competing dispatches never make stock negative or deduct twice')
    request('PUT', repair_path + '/trang-thai', tech,
        {'trangThai': 'HOAN_THANH', 'ketQuaSuaChua': 'Concurrency validation completed'})
    invoice = race('POST', '/api/hoa-don', reception, {'phieuId': repair_id, 'giamGia': 0}, 201, 'Concurrent invoice creation')
    check(invoice['tongThanhToan'] == 500, 'The winning invoice snapshots the single successful dispatch')
    invoice_path = '/api/hoa-don/' + str(invoice['id'])
    paid = race('POST', invoice_path + '/thanh-toan', reception, {'phuongThucTt': 'TIEN_MAT'}, 200, 'Concurrent invoice payment')
    check(paid['trangThai'] == 'DA_THANH_TOAN' and request('GET', invoice_path, reception)['trangThai'] == 'DA_THANH_TOAN',
          'The invoice remains paid after the rejected duplicate payment')
    movements = request('GET', '/api/ton-kho/bien-dong?khoId=' + str(warehouse['id']), keeper)
    check(len(movements) == 2 and {row['loai'] for row in movements} == {'NHAP', 'XUAT'}
          and sum(row['soLuongThayDoi'] for row in movements) == stock(), 'Stock ledger contains exactly one receipt and one dispatch')
    print('Concurrency smoke passed: ' + str(completed) + ' assertions. Generated local development records remain.', flush=True)


if __name__ == '__main__':
    try:
        main()
    except SmokeFailure as error:
        print('FAIL: ' + str(error), file=sys.stderr)
        sys.exit(1)
    except Exception as error:
        print('FAIL: unexpected ' + type(error).__name__ + ' (details suppressed)', file=sys.stderr)
        sys.exit(1)
