# Fitness Class Booking System - API Deployment Guide

This guide covers building and running the microservices as standalone containers using Docker Desktop on Windows.

## 🛠 Prerequisites

- Docker Desktop installed and running (Windows/Linux containers)
- PowerShell or a terminal of your choice
- .NET 9 SDK (if building locally outside Docker)

## 1. Accounts API

### Build the Image

Run this command from the `src` root directory to allow Docker to access the project dependencies (Domain, Application, Infrastructure):

```powershell
# Context: \FitnessClassBookingSystem\src
docker build -t fcbs-accounts:stable -f Accounts/Presentation/Dockerfile .
```

### Run the Container

Map the internal container ports to your local machine:

```powershell
docker run -d `
  -p 8080:8080 `
  -p 8081:8081 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  --name fcbs-accounts-api `
  fcbs-accounts:stable
```

### Access Swagger URL

**http://localhost:8080/swagger/index.html**

## 2. Classes API

### Build the Image

Run this from the `src` root directory:

```powershell
# Context: \FitnessClassBookingSystem\src
docker build -t fcbs-classes:stable -f Classes/Presentation/Dockerfile .
```

### Run the Container

Note that we use port `8090` for the host to avoid conflicts with the Accounts API:

```powershell
docker run -d `
  -p 8090:8080 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  --name fcbs-classes-api `
  fcbs-classes:stable
```

### Access Swagger URL

**http://localhost:8090/swagger/index.html**

## 💡 Quick Tips

### To view error logs of a deployment. Keep running this in a separate console window

```powershell
docker logs fcbs-classes-api -f
```

### Troubleshooting Container Conflicts

If you receive a "Name already in use" error, force-remove existing containers:

```powershell
docker rm -f fcbs-accounts-api
docker rm -f fcbs-classes-api
```

### Inter-Service Communication

When the Classes API needs to communicate with the Accounts API (both in Docker), use:

**Target Address**: [http://fcbs-accounts-api:8080](http://fcbs-accounts-api:8080)

### Useful Docker Commands

```powershell
# List running containers
docker ps

# View logs (follow mode)
docker logs -f fcbs-accounts-api

# Stop all running containers
docker stop $(docker ps -q)
```
