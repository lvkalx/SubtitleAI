# syntax=docker/dockerfile:1
# Build from the repository root:
#   docker build -f src/SubtitleAI.Api/Dockerfile -t subtitleai-api .

# ---------- 1. Whisper model (cached independently from source code) ----------
FROM debian:bookworm-slim AS model
ARG WHISPER_MODEL=ggml-base.bin
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /models \
    && curl -fL --retry 3 -o /models/${WHISPER_MODEL} \
       https://huggingface.co/ggerganov/whisper.cpp/resolve/main/${WHISPER_MODEL}

# ---------- 2. Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first so the restore layer is cached until dependencies change.
COPY src/SubtitleAI.Domain/SubtitleAI.Domain.csproj                 src/SubtitleAI.Domain/
COPY src/SubtitleAI.Application/SubtitleAI.Application.csproj       src/SubtitleAI.Application/
COPY src/SubtitleAI.Infrastructure/SubtitleAI.Infrastructure.csproj src/SubtitleAI.Infrastructure/
COPY src/SubtitleAI.Api/SubtitleAI.Api.csproj                       src/SubtitleAI.Api/
RUN dotnet restore src/SubtitleAI.Api/SubtitleAI.Api.csproj

COPY src/ src/
RUN dotnet publish src/SubtitleAI.Api/SubtitleAI.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- 3. Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# ffmpeg extracts audio; libgomp1 is required by Whisper's native OpenMP runtime.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg libgomp1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
COPY --from=model /models /models

# The runtime does not need write access to application binaries or the model.
RUN chown -R $APP_UID:$APP_UID /app /models

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Whisper__ModelPath=/models/ggml-base.bin

EXPOSE 8080

# Run as the non-root app user supplied by the ASP.NET image.
USER $APP_UID
ENTRYPOINT ["dotnet", "SubtitleAI.Api.dll"]
