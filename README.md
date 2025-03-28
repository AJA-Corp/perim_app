# Running the Project with Docker

This section provides instructions to build and run the project using Docker.

## Requirements

- Docker version 20.10 or later
- Docker Compose version 1.29 or later

## Build and Run Instructions

1. Clone the repository and navigate to the project root directory.
2. Build and start the services using Docker Compose:
   ```bash
   docker-compose up --build
   ```
3. The application will be available at `http://localhost:5000` (default port).

## Configuration

- The project uses .NET version 9.0 as specified in the Dockerfile.
- Ensure any required environment variables are set in a `.env` file or directly in the Compose file.

## Exposed Ports

- The application service exposes port `5000` for HTTP access.

For further details, refer to the project documentation or contact the development team.