FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG PROJECT_PATH

COPY QuanLyNganQuyCaNhan/QuanLyNganQuyCaNhan.API.csproj QuanLyNganQuyCaNhan/
COPY QuanLyNganQuyCaNhan.API.User/QuanLyNganQuyCaNhan.API.User.csproj QuanLyNganQuyCaNhan.API.User/
COPY QuanLyNganQuyCaNhan.API.Admin/QuanLyNganQuyCaNhan.API.Admin.csproj QuanLyNganQuyCaNhan.API.Admin/
COPY QuanLyNganQuyCaNhan.API.Gateway/QuanLyNganQuyCaNhan.API.Gateway.csproj QuanLyNganQuyCaNhan.API.Gateway/
COPY QuanLyNganQuyCaNhan.API.Shared/QuanLyNganQuyCaNhan.API.Shared.csproj QuanLyNganQuyCaNhan.API.Shared/
COPY QuanLyNganQuyCaNhan.BLL/QuanLyNganQuyCaNhan.BLL.csproj QuanLyNganQuyCaNhan.BLL/
COPY QuanLyNganQuyCaNhan.DAL/QuanLyNganQuyCaNhan.DAL.csproj QuanLyNganQuyCaNhan.DAL/

RUN dotnet restore "$PROJECT_PATH"

COPY . .
RUN dotnet publish "$PROJECT_PATH" -c Release \
    -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ARG APP_DLL
ENV APP_DLL=${APP_DLL}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV TZ=Asia/Bangkok

COPY --from=build /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["/bin/sh", "-c", "exec dotnet \"$APP_DLL\""]