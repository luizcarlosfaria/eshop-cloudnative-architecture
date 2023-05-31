FROM ubuntu:22.04


RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y \
    bzip2 \
    unzip \
    xz-utils \
    ca-certificates \
    ca-certificates-java \
    openjdk-17-jre-headless \
    dotnet7 \
    && rm -rf /var/lib/apt/lists/*
ENV LANG C.UTF-8



ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-sonarscanner
RUN dotnet tool install --global coverlet.console

RUN mkdir /root/.nuget/packages

RUN /var/lib/dpkg/info/ca-certificates-java.postinst configure