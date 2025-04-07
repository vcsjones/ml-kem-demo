FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04

RUN DEBIAN_FRONTEND=noninteractive \
    apt update && apt -y install --no-install-recommends \
    build-essential libicu-dev ca-certificates libc6 libgcc-s1 libicu74 liblttng-ust1 libssl3 libstdc++6 libunwind8 zlib1g

RUN mkdir -p $TMPDIR/openssl-build && cd $TMPDIR/openssl-build && \
    git clone https://github.com/openssl/openssl.git . && \
    git switch openssl-3.5 && \
    ./Configure -shared --prefix=/opt/openssl --openssldir=/opt/openssl && \
    make -j $(nproc) && \
    make install_sw && \
    cd ~ && rm -rf $TMPDIR/openssl-build