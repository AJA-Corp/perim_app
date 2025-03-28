# syntax=docker/dockerfile:1

# Define the .NET version to use
ARG DOTNET_VERSION=9.0

# Single stage build for Android
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

# Set the working directory
WORKDIR /src

# Install necessary packages
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    openjdk-17-jdk \
    && rm -rf /var/lib/apt/lists/*

# Set JAVA_HOME
ENV JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
ENV PATH=$PATH:$JAVA_HOME/bin

# Install Android SDK
ENV ANDROID_SDK_ROOT=/usr/local/android-sdk
ENV PATH=${PATH}:${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin
RUN mkdir -p ${ANDROID_SDK_ROOT} && \
    cd ${ANDROID_SDK_ROOT} && \
    wget https://dl.google.com/android/repository/commandlinetools-linux-10406996_latest.zip && \
    unzip commandlinetools-linux-*_latest.zip && \
    rm commandlinetools-linux-*_latest.zip && \
    mkdir -p cmdline-tools/latest && \
    mv cmdline-tools/* cmdline-tools/latest/ || true && \
    rm -rf cmdline-tools/latest/cmdline-tools && \
    yes | sdkmanager --licenses && \
    sdkmanager "platform-tools" "platforms;android-35" "build-tools;35.0.0"

# Install MAUI workloads
RUN dotnet workload install maui-android

# Copy the entire project directory
COPY ./PerimApp /src/PerimApp

# Verify the presence of critical files
RUN test -f /src/PerimApp/Resources/Splash/splash.svg || (echo "splash.svg not found" && exit 1)

# Build and publish the application
RUN cd /src/PerimApp && \
    dotnet restore && \
    dotnet build -f net9.0-android -c Release && \
    dotnet publish -f net9.0-android -c Release -o /app/publish

WORKDIR /app/publish

# The entry point should point to the Android APK
CMD ["ls", "-la", "/app/publish"]