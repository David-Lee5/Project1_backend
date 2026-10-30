Use the existing checkout /workspace/Project1_backend. Each task is already isolated; do not create a Git worktree unless explicitly requested. Read README.md and preserve unrelated user changes.

Prepared runtime:
- .NET SDK 8.0.427 and ASP.NET runtime 8.0.33: /workspace/.dotnet
- NuGet cache: /workspace/.nuget/packages
- Local setup helpers/state: /workspace/.setup/project1_backend
- Local development SQL/JWT credentials: private/local.json inside that directory (0600). Never print or copy its contents to repository/chat.
- SQL Server data bind directory: /workspace/.setup/project1_backend/mssql
- Retained, locally built SQL Server image: project1-sql:verified (Ubuntu 22.04 + Microsoft SQL Server 2022 package 16.0.4205.1-1 + mssql-tools18). Built from the official package after verifying Microsoft's signed repository index and package SHA256. The original MCR image pull was blocked by its CDN host; the verified official package route succeeded without disabling verification.
Live API and Docker processes must be started again after restoration.

Activate tools in new shells:
export DOTNET_ROOT=/workspace/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/workspace/.setup/project1_backend/dotnet-home
export NUGET_PACKAGES=/workspace/.nuget/packages
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

Refresh and build:
bash /workspace/.setup/project1_backend/install.sh

Start the prepared development workflow:
REPAIRSYSTEM_SQL_IMAGE=project1-sql:verified python3 /workspace/.setup/project1_backend/start.py --restart-api
The helper reuses only the labeled project1-backend-sql container at local port 1433, waits for an actual SQL query, and starts the built Debug API in Development on local port 5107. It injects the local connection string and JWT key into that process. It never overwrites nonempty unmarked databases. Do not rerun the dump into existing data. Its --restart-api option checks the saved API PID before terminating it.

If project1-sql:verified is absent after restoration, first check the managed local Docker socket using the runtime skill. As this task user is non-root, do not start/restart the daemon. Rebuild only from retained trusted files:
1. Verify /workspace/.setup/project1_backend/mssql-InRelease using the Microsoft keyring /workspace/.setup/project1_backend/gpg (key fingerprint BC528686B50D79E339D3721CEB3E94ADBE1229CF).
2. Confirm SHA256 of mssql-packages.gz matches its signed InRelease entry; confirm mssql-server.deb SHA256 matches the selected package record in that verified package index.
3. Build with inherited proxy and CA trust:
env -u DOCKER_HOST -u DOCKER_CONTEXT -u DOCKER_TLS -u DOCKER_TLS_VERIFY -u DOCKER_CERT_PATH docker --host=unix:///var/run/docker.sock build --secret id=proxy_ca,src=/etc/ssl/certs/ca-certificates.crt -t project1-sql:verified /workspace/.setup/project1_backend/sql-build
The Dockerfile uses BuildKit CA secret mode0444 for the _apt account and Microsoft keyring mode0644. Keep apt signatures and TLS checks enabled. Never copy the session CA into image layers.
Then run the startup helper again.

Required readiness checks:
cd /workspace/Project1_backend
dotnet test RepairSystem.sln --no-restore -m:1 --configuration Release
python3 /workspace/.setup/project1_backend/smoke.py --mode full --base-url http://127.0.0.1:5107
The unit/relational/test-host suite passed 109 tests in onboarding development. The local smoke verifies real SQL-backed catalog/auth/refresh/logout; it adds one local smoke customer. Check /health/ready separately; /health/live only proves process health. Do not expose local addresses as user-facing previews.

More complete real-SQL checks are in scripts/integration-smoke.py and scripts/concurrency-smoke.py. They require SMOKE_ADMIN_USERNAME and SMOKE_ADMIN_PASSWORD securely in their process environment; never hardcode or print them. They generate development employees, customers, stock and invoices and intentionally leave those records. Only use local development databases. The integration workflow and all four SQL concurrency races were exercised successfully in the current instance. These checks are optional for routine source-only tasks, required when changing their corresponding business workflows.

Repository features now include customers/devices, employee/catalog management and parts image metadata, repair reception/assignment/state history/service lines/photos, inventory receipts/dispatch/ledger, billing/payment, and auth ownership/active-account checks. Reuse database stored procedures for stock/invoice mutations; do not update stock totals directly. Money values use decimal(14,2), actor IDs derive from JWT staff profiles, and serialized transactions protect changes.

For a separate fresh development database use repository compose.yaml and scripts/init-database.py, as documented in README. The init script imports schema only by default, skips the dump's Windows paths and sample accounts, and preserves stored procedures. Configure Bootstrap__AdminPassword (12–100 chars) for the first admin on that clean database; existing admins are never reset. The repository appsettings deliberately contain no SQL password or signing key. .env is not loaded automatically by ASP.NET; use process variables or user secrets. CORS origins are explicit Cors__AllowedOrigins__0, etc. Cloudinary is not needed.

Do not assume the filesystem snapshot/configuration has been published or a fresh task has been validated merely because draft instructions are saved. Check current runtime readiness and services after restoration.
