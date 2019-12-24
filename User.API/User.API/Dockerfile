FROM microsoft/dotnet:sdk AS build-env
WORKDIR	/code
COPY *.csproj /code
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
WORKDIR /app
COPY --from=build-env /code/out ./
EXPOSE 80
ENTRYPOINT ["dotnet", "User.API.dll"]
