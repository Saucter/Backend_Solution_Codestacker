# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY . .
RUN dotnet restore 
RUN dotnet publish -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /publish
COPY --from=build /publish ./ 

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT [ "dotnet", "PDF_Reader_APIs.dll" ]doc