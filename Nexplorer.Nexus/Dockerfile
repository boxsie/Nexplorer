FROM ubuntu:latest

RUN apt-get update && apt-get -yq install git

RUN apt-get update && apt-get -yq install \
    build-essential libdb++-dev libssl-dev openssl

RUN git clone https://github.com/Nexusoft/LLL-TAO.git

ENV NEXUS_DEBUG 0
RUN cd /LLL-TAO && git checkout merging && make -j 8 -f makefile.cli ENABLE_DEBUG=$NEXUS_DEBUG

RUN mkdir /root/.TAO
COPY nexus.conf /root/.TAO/nexus.conf

CMD [ "/LLL-TAO/nexus" ]