# =======================================================
# Stage 1 - Build/compile app using container
# =======================================================

ARG IMAGE_BASE=3.1-alpine

# Build image has SDK and tools (Linux)
FROM mcr.microsoft.com/dotnet/core/sdk:$IMAGE_BASE as build
WORKDIR /build

# Copy project source files
COPY src ./src

# Restore, build & publish
WORKDIR /build/src
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release

# =======================================================
# Stage 2 - Assemble runtime image from previous stage
# =======================================================

# Base image is .NET Core runtime only (Linux)
FROM mcr.microsoft.com/dotnet/core/aspnet:$IMAGE_BASE

# Metadata in Label Schema format (http://label-schema.org)
LABEL org.label-schema.name    = ".NET Core Demo Web App" \
      org.label-schema.version = "1.3.3" \
      org.label-schema.vendor  = "Ben Coleman" \
      org.label-schema.vcs-url = "https://github.com/benc-uk/dotnet-demoapp"

# Seems as good a place as any
WORKDIR /app

# Copy already published binaries (from build stage image)
COPY --from=build /build/src/bin/Release/netcoreapp3.1/publish/ .

# Expose port 5000 from Kestrel webserver
EXPOSE 5000

# Tell Kestrel to listen on port 5000 and serve plain HTTP
ENV ASPNETCORE_URLS http://*:5000

# Run the ASP.NET Core app
ENTRYPOINT dotnet dotnet-demoapp.dll