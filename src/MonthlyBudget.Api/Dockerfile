FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY BudgetManager.sln ./
COPY src/MonthlyBudget.SharedKernel/MonthlyBudget.SharedKernel.csproj src/MonthlyBudget.SharedKernel/
COPY src/MonthlyBudget.Infrastructure/MonthlyBudget.Infrastructure.csproj src/MonthlyBudget.Infrastructure/
COPY src/MonthlyBudget.Api/MonthlyBudget.Api.csproj src/MonthlyBudget.Api/
COPY src/Modules/MonthlyBudget.BudgetManagement/MonthlyBudget.BudgetManagement.csproj src/Modules/MonthlyBudget.BudgetManagement/
COPY src/Modules/MonthlyBudget.ForecastEngine/MonthlyBudget.ForecastEngine.csproj src/Modules/MonthlyBudget.ForecastEngine/
COPY src/Modules/MonthlyBudget.IdentityHousehold/MonthlyBudget.IdentityHousehold.csproj src/Modules/MonthlyBudget.IdentityHousehold/

RUN dotnet restore src/MonthlyBudget.Api/MonthlyBudget.Api.csproj

# Copy full source
COPY . .

RUN dotnet build src/MonthlyBudget.Api/MonthlyBudget.Api.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish src/MonthlyBudget.Api/MonthlyBudget.Api.csproj -c Release --no-build -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MonthlyBudget.Api.dll"]

