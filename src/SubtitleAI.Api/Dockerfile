# syntax=docker/dockerfile:1
# Контекст сборки - корень репозитория:
#   docker build -f src/SubtitleAI.Api/Dockerfile -t subtitleai-api .

# ---------- 1. Модель Whisper (отдельная стадия: кешируется и не зависит от кода) ----------
FROM debian:bookworm-slim AS model
ARG WHISPER_MODEL=ggml-base.bin
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /models \
    && curl -fL --retry 3 -o /models/${WHISPER_MODEL} \
       https://huggingface.co/ggerganov/whisper.cpp/resolve/main/${WHISPER_MODEL}

# ---------- 2. Сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Сначала только .csproj - слой restore кешируется, пока не меняются зависимости
COPY src/SubtitleAI.Domain/SubtitleAI.Domain.csproj                 src/SubtitleAI.Domain/
COPY src/SubtitleAI.Application/SubtitleAI.Application.csproj       src/SubtitleAI.Application/
COPY src/SubtitleAI.Infrastructure/SubtitleAI.Infrastructure.csproj src/SubtitleAI.Infrastructure/
COPY src/SubtitleAI.Api/SubtitleAI.Api.csproj                       src/SubtitleAI.Api/
RUN dotnet restore src/SubtitleAI.Api/SubtitleAI.Api.csproj

COPY src/ src/
RUN dotnet publish src/SubtitleAI.Api/SubtitleAI.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- 3. Рантайм ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# ffmpeg - извлечение аудио, libgomp1 - нужна нативному рантайму Whisper (OpenMP)
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg libgomp1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
COPY --from=model /models /models

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Whisper__ModelPath=/models/ggml-base.bin

EXPOSE 8080

# Не root (пользователь app уже есть в образе aspnet)
USER $APP_UID
ENTRYPOINT ["dotnet", "SubtitleAI.Api.dll"]
