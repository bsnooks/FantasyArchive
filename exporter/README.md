# Fantasy Archive Exporter

## Setup

1. **Database Configuration**: Copy `appsettings.Example.json` to `appsettings.json` and update the connection string:

```bash
copy appsettings.Example.json appsettings.json
```

2. **Edit appsettings.json** with your database details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "ExportSettings": {
    "OutputPath": "./exports"
  }
}
```

3. **Run the exporter**:

```bash
dotnet run
```

## Configuration

- **ConnectionStrings:DefaultConnection**: SQL Server connection string
- **ExportSettings:OutputPath**: Directory where exported JSON files will be saved

## Security

The `appsettings.json` file is excluded from git to protect sensitive connection strings. Use `appsettings.Example.json` as a template for new setups.

## Output

The exporter generates organized JSON files in the following structure:

```
exports/
├── franchises/           # Individual franchise data with all-time rosters
├── seasons/             # Season data by year
└── records/             # League records
    ├── all-time/        # All-time league records
    ├── seasons/         # Records by season
    └── franchises/      # Records by franchise
```