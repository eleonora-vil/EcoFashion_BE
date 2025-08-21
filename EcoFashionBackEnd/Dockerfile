# Stage 1: Build the application (using .NET SDK 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the build configuration (can be overridden by a build argument)
ARG BUILD_CONFIGURATION=Release

# Install dependencies required for native AOT publishing (e.g., clang, zlib1g-dev)
# These are necessary if you're compiling to a self-contained native executable.
# If you're not using AOT, these might not be strictly necessary, but included for consistency with the video.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev

# Set the working directory inside the container
WORKDIR /src

# Copy the .csproj file first to leverage Docker's layer caching for NuGet restore
# ASSUMPTION: Your EcoFashionBackEnd.csproj is in the root of your repository.
# If your .csproj is in a subfolder (e.g., EcoFashionBackEnd/EcoFashionBackEnd.csproj),
# you might need to adjust this line to: COPY ["EcoFashionBackEnd/EcoFashionBackEnd.csproj", "EcoFashionBackEnd/"]
COPY EcoFashionBackEnd.csproj ./

# Restore NuGet packages
# This step fetches all dependencies required by your project.
RUN dotnet restore "EcoFashionBackEnd.csproj"

# Copy the rest of your application code
# This copies all other files from your local directory into the container.
COPY . .

# Build the application
# This compiles your source code.
# ASSUMPTION: If your .csproj is in a subfolder, adjust the path (e.g., "EcoFashionBackEnd/EcoFashionBackEnd.csproj")
RUN dotnet build "EcoFashionBackEnd.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish the application
# This stage takes the built application and prepares it for deployment.
FROM build AS publish

# Set the build configuration again for consistency
ARG BUILD_CONFIGURATION=Release

# Publish the application as a self-contained executable (using /p:UseAppHost=true for AOT)
# The --no-restore flag is NOT used here, allowing publish to restore if needed (though already done).
# If your .csproj is in a subfolder, adjust the path (e.g., "EcoFashionBackEnd/EcoFashionBackEnd.csproj")
RUN dotnet publish "EcoFashionBackEnd.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Stage 3: Create the final runtime image (minimal image for self-contained app)
# Using runtime-deps as the app is self-contained and doesn't need the full ASP.NET runtime installed separately.
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS final

# Set the working directory for the final application
WORKDIR /app

# Expose the port your application listens on (8080 is common for Railway)
EXPOSE 8080

# Copy the published application from the 'publish' stage to the final image
COPY --from=publish /app/publish .

# Set the entry point for your application
# The entry point is the name of your executable (typically your project name).
ENTRYPOINT ["./EcoFashionBackEnd"]