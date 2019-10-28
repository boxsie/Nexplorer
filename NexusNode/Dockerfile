FROM debian:latest

RUN apt-get update && apt-get install -y --no-install-recommends \
	build-essential \
	ca-certificates \
    git

RUN apt-get update && apt-get install -y --no-install-recommends \
    libdb++-dev \
	libssl-dev openssl

RUN git clone https://github.com/Nexusoft/LLL-TAO.git
RUN cd /LLL-TAO \
	&& git checkout testnet && make -f makefile.cli

RUN mkdir /root/.TAO
COPY Nexus/nexus.conf /root/.TAO/nexus.conf

CMD ["/LLL-TAO/nexus"]