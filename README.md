This is a simple demonstration of the new `MLKem` class in .NET 10. It is not meant to be a demonstation of how to use ML-KEM safe or effectively.

To use this demo, you need at least OpenSSL 3.5. This repository contains a codespace that sets up OpenSSL 3.5. This demo shows two different users, "penny" and "vince", sending messages to create an ML-KEM shared secret.

In one terminal window, run as vince using `dotnet run -- vince`. Vince will wait for Penny to receive data. In a different terminal window, run as penny using `dotnet run -- penny`.