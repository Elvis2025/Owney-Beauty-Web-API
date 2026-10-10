# syntax=docker/dockerfile:1
#
# Owney Beauty — imagen única para Railway (DEV-F01).
# Contiene tres procesos; cada servicio de Railway elige cuál ejecutar con su startCommand:
#   API      -> /app/start-api.sh       (escucha en $PORT)
#   Worker   -> /app/start-worker.sh    (outbox, inbox, jobs, correos)
#   Migrador -> /app/start-migrator.sh  (preDeployCommand del servicio API)
# La misma imagen se promueve entre entornos (DEVOPS-01-002).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Node 24 (requerido por las librerías de login, p. ej. select2) + ABP CLI, solo para restaurar
# las librerías estáticas de las páginas de login (wwwroot/libs). No llegan a la imagen final.
COPY --from=node:24-bookworm-slim /usr/local/bin/node /usr/local/bin/node
COPY --from=node:24-bookworm-slim /usr/local/lib/node_modules /usr/local/lib/node_modules
RUN ln -s /usr/local/lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm \
    && ln -s /usr/local/lib/node_modules/npm/bin/npx-cli.js /usr/local/bin/npx \
    && npm install --global yarn@1.22.22 \
    && dotnet tool install --global Volo.Abp.Studio.Cli --version 3.1.1
ENV PATH="${PATH}:/root/.dotnet/tools"

# Restaurar primero (capa cacheable).
COPY common.props NuGet.Config Ieh.OwneyBeauty.slnx ./
COPY src/ src/
RUN dotnet restore src/Ieh.OwneyBeauty.HttpApi.Host/Ieh.OwneyBeauty.HttpApi.Host.csproj \
    && dotnet restore src/Ieh.OwneyBeauty.Worker/Ieh.OwneyBeauty.Worker.csproj \
    && dotnet restore src/Ieh.OwneyBeauty.DbMigrator/Ieh.OwneyBeauty.DbMigrator.csproj

RUN cd src/Ieh.OwneyBeauty.HttpApi.Host && abp install-libs

RUN dotnet publish src/Ieh.OwneyBeauty.HttpApi.Host/Ieh.OwneyBeauty.HttpApi.Host.csproj -c Release -o /out/api --no-restore /p:UseAppHost=false \
    && dotnet publish src/Ieh.OwneyBeauty.Worker/Ieh.OwneyBeauty.Worker.csproj -c Release -o /out/worker --no-restore /p:UseAppHost=false \
    && dotnet publish src/Ieh.OwneyBeauty.DbMigrator/Ieh.OwneyBeauty.DbMigrator.csproj -c Release -o /out/migrator --no-restore /p:UseAppHost=false \
    && find /out -name "appsettings.secrets.json" -delete

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_gcServer=1

COPY --from=build /out/api ./api
COPY --from=build /out/worker ./worker
COPY --from=build /out/migrator ./migrator

RUN printf '#!/bin/sh\nset -e\ncd /app/api\nexport ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"\nexport App__HealthUiCheckUrl="${App__HealthUiCheckUrl:-http://localhost:${PORT:-8080}/health-status}"\nexec dotnet Ieh.OwneyBeauty.HttpApi.Host.dll\n' > /app/start-api.sh \
    && printf '#!/bin/sh\nset -e\ncd /app/worker\nexec dotnet Ieh.OwneyBeauty.Worker.dll\n' > /app/start-worker.sh \
    && printf '#!/bin/sh\nset -e\ncd /app/migrator\nexec dotnet Ieh.OwneyBeauty.DbMigrator.dll\n' > /app/start-migrator.sh \
    && chmod +x /app/*.sh \
    && mkdir -p /app/api/Logs /app/migrator/Logs \
    && chown -R $APP_UID /app/api/Logs /app/migrator/Logs

USER $APP_UID
EXPOSE 8080
CMD ["/app/start-api.sh"]
