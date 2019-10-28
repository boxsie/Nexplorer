FROM debian:latest

RUN apt-get update && apt-get install -y --no-install-recommends \
	build-essential \
    supervisor \
	ca-certificates \
    apt-transport-https \
    git \
    nginx \
	wget \
	gpg

RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
    && mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
    && wget -q https://packages.microsoft.com/config/debian/10/prod.list \
    && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
    && chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
    && chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN apt-get update && apt-get install -y --no-install-recommends \
    dotnet-sdk-3.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    libdb++-dev \
	libssl-dev openssl

RUN git clone https://github.com/Nexusoft/LLL-TAO.git
RUN cd /LLL-TAO \
	&& git checkout testnet && make -f makefile.cli

RUN mkdir /Nexplorer
COPY / /Nexplorer
RUN cd /Nexplorer/Nexplorer.Node && dotnet publish -c Release -o /Nexplorer/node

RUN mkdir /root/.TAO
COPY NodeContainer/Nexus/nexus.conf /root/.TAO/nexus.conf

COPY NodeContainer/nginx/default /etc/nginx/sites-available/default
COPY NodeContainer/supervisor/supervisor.conf /etc/supervisor/conf.d/supervisor.conf

CMD ["/usr/bin/supervisord"]