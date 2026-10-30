#!/usr/bin/env python3
"""Initialize an empty local SQL Server database without changing the exported SQL.

Provide SQLCMDPASSWORD in the process environment. By default imports schema only;
--sample-data also imports the export's demonstration rows (not working credentials).
Existing nonempty databases are preserved and rejected; never resets data.
Use --output to export a SQL file for SSMS without Docker or a running database.
"""
import argparse
import os
from pathlib import Path
import re
import subprocess
import sys


def schema_batches(source, samples=False):
    source = source[source.index('USE [QLSCTBDT]'):]
    for batch in re.split(r'^GO\s*$', source, flags=re.MULTILINE):
        # Skip only top-level data export batches, never INSERTs inside procedures.
        head = re.sub(r'/\*.*?\*/|--[^\n]*', '', batch, flags=re.DOTALL).lstrip()
        if not samples and re.match(r'(?:INSERT\s+\[dbo\]|SET\s+IDENTITY_INSERT\b)', head, re.IGNORECASE):
            continue
        if batch.strip():
            yield batch


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--container', default='repairsystem-sql')
    parser.add_argument('--database', default='QLSCTBDT')
    parser.add_argument('--sample-data', action='store_true')
    parser.add_argument('--output', type=Path, help='Export SQL for an empty database; no Docker connection or password required.')
    options = parser.parse_args()
    if not re.fullmatch(r'[A-Za-z][A-Za-z0-9_]{0,100}', options.database):
        parser.error('Database name must contain only letters, numbers and underscores.')
    source = (Path(__file__).resolve().parents[1] / 'data/data_v2.sql').read_text(encoding='utf-16')
    sql = '\nGO\n'.join(schema_batches(source, options.sample_data)) + '\nGO\n'
    sql = sql.replace('[QLSCTBDT]', f'[{options.database}]')
    if options.output:
        options.output.parent.mkdir(parents=True, exist_ok=True)
        # SSMS detects UTF-8 reliably from its BOM; preserve any existing output file.
        with options.output.open('x', encoding='utf-8-sig') as output:
            output.write(sql)
        print(f'SQL exported to {options.output}. Create an empty {options.database} database in SSMS, then execute the whole file.')
        return
    if not os.environ.get('SQLCMDPASSWORD'):
        parser.error('Set SQLCMDPASSWORD securely in the process environment.')
    environment = dict(os.environ)
    for name in ('DOCKER_HOST', 'DOCKER_CONTEXT', 'DOCKER_TLS', 'DOCKER_TLS_VERIFY', 'DOCKER_CERT_PATH'):
        environment.pop(name, None)

    def query(text, database='master'):
        command = ['docker', '--host=unix:///var/run/docker.sock', 'exec', '-i', '-e', 'SQLCMDPASSWORD', options.container,
                   '/opt/mssql-tools18/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-C', '-I', '-b', '-h', '-1', '-W', '-d', database]
        result = subprocess.run(command, input=text, text=True, capture_output=True, env=environment)
        if result.returncode:
            raise RuntimeError('SQL command failed. Check database readiness and server logs locally; existing data was not reset.')
        return result.stdout.strip()

    name = options.database
    exists = query(f"SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'{name}') IS NULL THEN 0 ELSE 1 END")
    if exists == '0':
        query(f'CREATE DATABASE [{name}]')
    if query('SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables', name) != '0':
        raise RuntimeError('Database is not empty. Refusing to overwrite existing data. Use an empty database or review a migration.')
    query(sql, name)
    required = query("SET NOCOUNT ON; SELECT COUNT(*) FROM sys.procedures WHERE name IN ('usp_xac_nhan_nhap_kho','usp_xuat_kho','usp_lap_hoa_don')", name)
    if required != '3':
        raise RuntimeError('Import is incomplete: required business procedures are missing. Preserve the database for inspection.')
    print('Database schema initialized; all three business procedures verified.' + (' Demonstration rows included.' if options.sample_data else ' No demonstration accounts or records imported.'))


if __name__ == '__main__':
    try:
        main()
    except (RuntimeError, OSError, ValueError) as error:
        print('ERROR: ' + str(error), file=sys.stderr)
        sys.exit(1)
