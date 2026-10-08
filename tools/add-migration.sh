dotnet restore MiniProject.slnx --verbosity quiet
dotnet ef migrations add InitialPortfolioSchema --project MiniProject.Migrations/
MiniProject.Migrations.csproj --startup-project MiniProject.Playground/MiniProject.Playground.csproj --context MiniDbContext