# Сгенерируйте новую миграцию
```
dotnet ef migrations add InitialMigration
```

# Примените миграции
```
dotnet ef database update
```

docker exec -it investlens.data.api dotnet ef database update