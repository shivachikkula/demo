# District Portal API

A .NET 10 / ASP.NET Core Web API backing the Angular District Portal app - GET/POST endpoints
for both the Sponsor (single-LEA) view and the Admin (all-LEAs) view, file uploads to Azure
Blob Storage, and a SignalR hub that replaces polling with a live push once a file finishes
validating.

## Stack

- **.NET 10** / ASP.NET Core (controllers, not minimal APIs, given the size of the surface)
- **EF Core 10 + SQLite** for persistence (swap the `Sqlite` connection string/provider for SQL
  Server in production - nothing else in the code depends on SQLite)
- **Azure.Storage.Blobs** for file storage
- **SignalR** for real-time submission-status push
- **Scalar** (`/scalar/v1`) for interactive API docs in Development, backed by the built-in
  `Microsoft.AspNetCore.OpenApi` document at `/openapi/v1.json`

## Running it locally

1. **Blob storage**: point `BlobStorage:ConnectionString` (in `appsettings.json` or user-secrets)
   at a real Storage Account connection string, or run the
   [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) emulator
   locally and leave the default `UseDevelopmentStorage=true`:
   ```bash
   npx azurite --silent --location ./.azurite
   ```
2. **Database**: SQLite is file-based and needs no setup - migrations run automatically on
   startup (`db.Database.Migrate()` in `Program.cs`), and the database is seeded with the same
   demo data the Angular app ships as mock data (DCPS, KIPP DC, Friendship PCS, UDC-CC).
3. **Run**:
   ```bash
   cd src/DistrictPortal.Api
   dotnet run --urls "http://localhost:5080"
   ```
4. Browse `http://localhost:5080/scalar/v1` for interactive docs, or hit the endpoints below
   directly. CORS is open for `http://localhost:4200` (the Angular dev server) with credentials
   allowed, which SignalR's negotiate handshake requires.

## Endpoints

All routes are relative to the API base URL.

### LEA dashboard - `api/leas` (shared by Sponsor and Admin's drill-in page)

| Method | Route | Notes |
|---|---|---|
| GET | `/api/leas/{leaId}` | Active/inactive collection summaries for one LEA |
| GET | `/api/leas/{leaId}/collections/{collectionId}` | Stats, last uploaded file, submission history |
| GET | `/api/leas/{leaId}/collections/{collectionId}/submissions` | Submission history only |
| POST | `/api/leas/{leaId}/collections/{collectionId}/submissions` | `multipart/form-data`: `file`, optional `uploadedBy`. Returns `202 Accepted` with status `Processing` immediately; the real result arrives over SignalR (see below) |
| GET | `/api/leas/{leaId}/collections/{collectionId}/submissions/{submissionId}/download` | Streams the file back from blob storage |

The Sponsor view always calls these with its own fixed `leaId`; the Admin view's drill-in page
calls them with whichever LEA the admin selected from the roster - same endpoints, same
component on the frontend, just a different id.

### Admin roster - `api/admin`

| Method | Route | Notes |
|---|---|---|
| GET | `/api/admin/leas?search=&sortBy=&sortDir=` | Aggregate district-wide stats plus a per-LEA row. `search` matches LEA name **or** any of its collection names (mirrors the frontend's search-by-collection feature). `sortBy` is `name` \| `overdueCount` \| `failedCount` \| `processingCount`; `sortDir` is `asc` \| `desc` |

### Notifications - `api/notifications` (shared bell menu)

| Method | Route | Notes |
|---|---|---|
| GET | `/api/notifications` | All notifications, newest first |
| POST | `/api/notifications/{id}/read` | Mark one read |
| POST | `/api/notifications/read-all` | Mark all read |
| DELETE | `/api/notifications/{id}` | Dismiss |

## Real-time file processing (replaces 5-minute polling)

`POST .../submissions` no longer requires the client to poll for a result. Instead:

1. The endpoint stores the file in blob storage, inserts a `Submissions` row with
   `Status = Processing`, `UploadedBy`, `UploadedAt`, and a `Guid.CreateVersion7()` id, and
   returns `202 Accepted` right away.
2. A background worker (`SubmissionProcessingWorker`, an `IHostedService`) picks the job up off
   an in-process channel, simulates validation (a few seconds), and updates the row with the
   final `Passed`/`Failed` status and record counts.
3. The worker pushes the result over the `FileProcessingHub` SignalR hub
   (`/hubs/file-processing`):
   - `SubmissionStatusChanged` to the group `lea:{leaId}` - the payload carries both the updated
     submission and the collection's fully refreshed detail (stats, last file, history), so a
     client watching that LEA's dashboard can just replace its state wholesale.
   - `NotificationCreated` to the `global` group (every connected client) - so the bell badge
     updates immediately, matching the "notify the user once the file processes" ask.

A minimal Angular client would do:

```ts
const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5080/hubs/file-processing')
  .withAutomaticReconnect()
  .build();

connection.on('SubmissionStatusChanged', (msg) => { /* update collection detail signal */ });
connection.on('NotificationCreated', (notification) => { /* prepend to notifications signal */ });

await connection.start();
await connection.invoke('JoinLeaGroup', leaId); // call again on every LEA route change
```

This isn't wired into the Angular app yet - the app still uses in-memory mock data. Swapping
the mock arrays in `dashboard.ts` / `admin-dashboard.ts` for `HttpClient` calls against this API,
plus adding the SignalR client above, is the natural next step.
