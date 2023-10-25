# development
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS development

RUN mkdir -p /home/dotnet/FFC.Payment.Integrations.Function.Tests/
RUN mkdir -p /home/dotnet/FFC.Payment.Integrations.Function.Tests/ /home/dotnet/FFC.Payment.Integrations.Function/
COPY --chown=dotnet:dotnet ./FFC.Payment.Integrations.Function.Tests/*.csproj ./FFC.Payment.Integrations.Function.Tests/
RUN dotnet restore ./FFC.Payment.Integrations.Function.Tests/FFC.Payment.Integrations.Function.Tests.csproj
COPY --chown=dotnet:dotnet ./FFC.Payment.Integrations.Function/*.csproj ./FFC.Payment.Integrations.Function/
RUN dotnet restore ./FFC.Payment.Integrations.Function/FFC.Payment.Integrations.Function.csproj

COPY ./FFC.Payment.Integrations.Function /src
RUN cd /src && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4.27.5-dotnet-isolated6.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=development ["/home/site/wwwroot", "/home/site/wwwroot"]


# production
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS production

COPY ./FFC.Payment.Integrations.Function /src
RUN cd /src && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4.27.5-dotnet-isolated6.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=production ["/home/site/wwwroot", "/home/site/wwwroot"]