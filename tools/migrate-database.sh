#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"
dotnet ef database update \
  --project "$ROOT_DIR/MiniProject.Migrations/MiniProject.Migrations.csproj" \
  --startup-project "$ROOT_DIR/MiniProject.Playground/MiniProject.Playground.csproj" \
  --context MiniDbContext
