#pragma warning disable SYSLIB5006
using System.IO.Pipes;
using System.Security.Cryptography;

MLKemAlgorithm algorithm = MLKemAlgorithm.MLKem768;
const string PipeName = "mlkem-pipe";

if (args is ["vince"]) {
    Console.WriteLine($"Generating {algorithm.Name} key...");

    // Generate an ML-KEM key
    using MLKem kem = MLKem.GenerateKey(algorithm);

    // Get the (public) encapsulation key.
    byte[] encapsulationKey = kem.ExportEncapsulationKey();

    Console.WriteLine($"Encapsulation Key SHA-2-256: {Convert.ToHexString(SHA256.HashData(encapsulationKey))}");
    Console.WriteLine("Waiting for penny to receive encapsulation key...");

    using NamedPipeServerStream pipe = new(PipeName, PipeDirection.InOut, 1);
    await pipe.WaitForConnectionAsync();

    // Send the encapsulation key to penny
    pipe.Write(encapsulationKey);

    Console.WriteLine("Waiting for penny's ciphertext...");
    byte[] ciphertext = new byte[algorithm.CiphertextSizeInBytes];

    while (pipe.IsConnected)
    {
        try
        {
            // Get the ciphertext from penny
            pipe.ReadExactly(ciphertext);
            Console.WriteLine($"Received penny's ciphertext: {Convert.ToHexString(ciphertext)}");

            // Decapsulate the ciphertext that penny made.
            byte[] sharedSecret = kem.Decapsulate(ciphertext);
            Console.WriteLine($"Shared secret: {Convert.ToHexString(sharedSecret)}");
            Console.WriteLine("Waiting for penny to send more.");
        }
        catch (EndOfStreamException)
        {
            break;
        }
    }

    Console.WriteLine("penny is done.");
}
else if (args is ["penny"]) {
    using NamedPipeClientStream pipe = new(".", PipeName, PipeDirection.InOut);
    Console.WriteLine("Receiving encapsulation key...");
    await pipe.ConnectAsync();
    byte[] encapsulationKey = new byte[algorithm.EncapsulationKeySizeInBytes];
    pipe.ReadExactly(encapsulationKey);
    Console.WriteLine($"Encapsulation Key SHA-2-256: {Convert.ToHexString(SHA256.HashData(encapsulationKey))}");
    Console.WriteLine("Encapsulating...");

    // Get the encapsulation key from vince
    using MLKem kem = MLKem.ImportEncapsulationKey(algorithm, encapsulationKey);

    do
    {
        Console.WriteLine();
        // Encapsulate a ciphertext with vince's encapsulation key
        byte[] ciphertext = kem.Encapsulate(out byte[] sharedSecret);

        Console.WriteLine($"Sending ciphertext to vince: {Convert.ToHexString(ciphertext)}");

        // Sent the ciphertext back to vince.
        pipe.Write(ciphertext);
        Console.WriteLine("Sent ciphertext.");
        Console.WriteLine($"Shared secret: {Convert.ToHexString(sharedSecret)}");
        Console.Write("Again? [yY]");
    }
    while (Console.ReadKey(true) is { KeyChar: 'y' or 'Y' });

    Console.WriteLine();
    Console.WriteLine("Done.");
}
else {
    Console.WriteLine("Run as either 'vince' or 'penny'.");
}