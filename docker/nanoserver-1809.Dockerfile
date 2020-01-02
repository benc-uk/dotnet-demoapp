# =======================================================
# Stage 1 - Build/compile app using container
# =======================================================

# Build image has SDK and tools (Windows Nano Server 1803)
FROM mcr.microsoft.com/dotnet/core/sdk:2.2-nanoserver-1809 as build
WORKDIR /build

# Copy project source files
COPY wwwroot ./wwwroot
COPY Pages ./Pages
COPY *.cs ./
COPY *.json ./
COPY *.csproj ./

# Restore, build & publish
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release

# =======================================================
# Stage 2 - Assemble runtime image from previous stage
# =======================================================

# Base image is .NET Core runtime only (Windows Nano Server 1803)
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-nanoserver-1809

# Metadata in Label Schema format (http://label-schema.org)
LABEL org.label-schema.name    = ".NET Core Demo Web App" \
      org.label-schema.version = "3.5.4" \
      org.label-schema.vendor  = "Ben Coleman" \
      org.label-schema.vcs-url = "https://github.com/benc-uk/dotnet-demoapp"

# Seems as good a place as any
WORKDIR /app

# Copy already published binaries (from build stage image)
COPY --from=build /build/bin/Release/netcoreapp2.2/publish .

# Flag file to indicate to code at runtime it is inside a container
COPY .insidedocker .

# Expose port 5000 from Kestrel webserver
EXPOSE 5000

# Run the ASP.NET Core app
ENTRYPOINT dotnet dotnet-demoapp.dll
