# Ensek Remote Technical Test

## How to Run the Project

### Prerequisites
- Docker and Docker Compose
- .NET SDK
- Node.js and npm

### Steps

1. **Start PostgreSQL database**
   ```bash
   docker compose up -d
   ```

2. **Run the backend API**
   ```bash
   dotnet run --project src/backend/Api
   ```

3. **Run the frontend**
   ```bash
   # Navigate to frontend directory
   cd src/frontend
   npm run dev
   ```

### Testing

**Backend tests:**
```bash
dotnet test
```

**Frontend E2E tests:**
```bash
# Note that the api should be running for the E2E tests to work.
cd src/frontend
npm run test:e2e
```