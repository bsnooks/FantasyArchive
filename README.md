# FantasyArchive Project

This repository contains a multi-part application for extracting and archiving fantasy league data for "Gibsons League".

## Structure

- `data/` - .NET class library with Entity Framework models and JSON export models
- `exporter/` - .NET console app for exporting data from SQL Server database to JSON files
- `frontend/` - React TypeScript app for displaying franchise and season history
- `backend/` - ASP.NET Core Web API (future: for managing data sync and authentication)

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+ recommended)](https://nodejs.org/)
- SQL Server (LocalDB or full instance)

### Quick Start
```bash
# Start the React frontend with sample data
./run-all.ps1
```

### Manual Setup

#### Frontend (React App)
```bash
cd frontend
npm install
npm start
```
Opens in browser at http://localhost:3000

#### Data Exporter
```bash
cd exporter
# Update connection string in Program.cs if needed
dotnet run
```
Exports JSON files to `./exports/` directory from your SQL Server database.

#### Data Models
```bash
cd data
dotnet build
```

## Features

### Current Implementation
- ✅ React frontend displaying franchises and seasons
- ✅ Data models for Franchise, Season, and Team entities
- ✅ JSON export functionality from SQL Server
- ✅ Responsive UI with franchise and season views
- ✅ Sample data for testing

### Future Enhancements
- [ ] Database seeding and migrations
- [ ] Yahoo Fantasy Sports API integration
- [ ] OAuth2 authentication flow
- [ ] Real-time data sync
- [ ] Player statistics and matchup data
- [ ] Advanced analytics and charts

## Data Structure

### Franchises
- Basic franchise information (name, owner, established date)
- Historical team names and records by season
- Active/inactive status

### Seasons
- Season details (year, start/end dates)
- Team standings and records
- Points for/against statistics

## Development

### Adding New Data
1. Update Entity Framework models in `data/Models/`
2. Create corresponding JSON models in `data/JsonModels/`
3. Update exporter logic in `exporter/Program.cs`
4. Add React components/types in `frontend/src/`

### Database Schema
The application expects a SQL Server database with tables for:
- Franchises
- Seasons  
- Teams (linking franchises to seasons with stats)

---

For questions or contributions, please open an issue or pull request.