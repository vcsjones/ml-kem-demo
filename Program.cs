#pragma warning disable SYSLIB5006
using System.IO.Pipes;
using System.Security.Cryptography;

MLKemAlgorithm algorithm = MLKemAlgorithm.MLKem768;
const string PipeName = "mlkem-pipe";

if (args is ["vince"]) {
    Console.WriteLine("Generating ML-KEM-768 key...");
    using MLKem kem = MLKem.GenerateKey(algorithm);
    byte[] encapsulationKey = kem.ExportEncapsulationKey();
    Console.WriteLine($"Encapsulation Key SHA-2-256: {Convert.ToHexString(SHA256.HashData(encapsulationKey))}");
    Console.WriteLine("Waiting for penny to receive encapsulation key...");

    using NamedPipeServerStream pipe = new(PipeName, PipeDirection.InOut, 1);
    await pipe.WaitForConnectionAsync();
    pipe.Write(encapsulationKey);

    Console.WriteLine("Waiting for penny's ciphertext...");
    byte[] ciphertext = new byte[algorithm.CiphertextSizeInBytes];
    pipe.ReadExactly(ciphertext);
    Console.WriteLine($"Received penny's ciphertext: {Convert.ToHexString(ciphertext)}");

    byte[] sharedSecret = kem.Decapsulate(ciphertext);
    Console.WriteLine($"Shared secret: {Convert.ToHexString(sharedSecret)}");
}
else if (args is ["penny"]) {
    using NamedPipeClientStream pipe = new(".", PipeName, PipeDirection.InOut);
    await pipe.ConnectAsync();
    Console.WriteLine("Receiving encapsulation key...");
    byte[] encapsulationKey = new byte[algorithm.EncapsulationKeySizeInBytes];
    pipe.ReadExactly(encapsulationKey);
    Console.WriteLine($"Encapsulation Key SHA-2-256: {Convert.ToHexString(SHA256.HashData(encapsulationKey))}");
    Console.WriteLine("Encapsulating...");
    using MLKem kem = MLKem.ImportEncapsulationKey(algorithm, encapsulationKey);
    byte[] ciphertext = kem.Encapsulate(out byte[] sharedSecret);

    Console.WriteLine($"Sending ciphertext to vince: {Convert.ToHexString(ciphertext)}");
    pipe.Write(ciphertext);
    Console.WriteLine("Sent ciphertext.");
    Console.WriteLine($"Shared secret: {Convert.ToHexString(sharedSecret)}");
}
else {
    Console.WriteLine("Run as either. 'vince' or 'penny'.");
}