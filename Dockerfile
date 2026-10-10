FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MiniProject.Migrations/MiniProject.Migrations.csproj MiniProject.Migrations/
COPY MiniProject.ComplexLogicInMiddle/MiniProject.ComplexLogicInMiddle.csproj MiniProject.ComplexLogicInMiddle/
COPY MiniProject.Playground/MiniProject.Playground.csproj MiniProject.Playground/
RUN dotnet restore MiniProject.Playground/MiniProject.Playground.csproj

COPY MiniProject.Migrations/ MiniProject.Migrations/
COPY MiniProject.ComplexLogicInMiddle/ MiniProject.ComplexLogicInMiddle/
COPY MiniProject.Playground/ MiniProject.Playground/
RUN dotnet publish MiniProject.Playground/MiniProject.Playground.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniProject.Playground.dll"]
