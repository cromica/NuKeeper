FROM microsoft/dotnet:2.2-sdk
RUN dotnet tool install --global NuKeeper --version 0.19.0
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["nukeeper"]
