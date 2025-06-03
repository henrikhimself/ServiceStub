using System.Security.Cryptography.X509Certificates;

namespace Hj.ServiceStub.Certificate.Store;

internal sealed class FileSystemStore(
  ILogger<FileSystemStore> logger,
  CertificateFactory certificateFactory) : ICertificateStore
{
  public X509Certificate2? LoadCa(SelfSignedOptions options)
  {
    GetCaFilePaths(options, out var caCrtPemFilePath, out var caKeyPemFilePath, out _);

    if (!File.Exists(caCrtPemFilePath) || !File.Exists(caKeyPemFilePath))
    {
      logger.LogInformation("Missing CA, cert path '{CertPemPath}', key path '{CaKeyPath}'", caCrtPemFilePath, caKeyPemFilePath);
      return null;
    }

    logger.LogInformation("Loading CA, cert path '{CertPemPath}', key path '{CaKeyPath}'", caCrtPemFilePath, caKeyPemFilePath);
    var ca = X509Certificate2.CreateFromPemFile(caCrtPemFilePath, caKeyPemFilePath);
    return ca;
  }

  public void SaveCa(SelfSignedOptions options, X509Certificate2 ca)
  {
    GetCaFilePaths(options, out var caCrtPemFilePath, out var caKeyPemFilePath, out var caPfxFilePath);
    logger.LogInformation("Saving CA, cert path '{CertPemPath}', key path '{CaKeyPemPath}', pfx path '{CaPfxPath}'", caCrtPemFilePath, caKeyPemFilePath, caPfxFilePath);

    File.WriteAllText(caCrtPemFilePath, ca.ExportCertificatePem());
    File.WriteAllText(caKeyPemFilePath, certificateFactory.ExportPrivateKeyPem(ca));

    // Intentionally skipping adding a password here to make it easier to import ca into a trusted root ca store.
    File.WriteAllBytes(caPfxFilePath, ca.Export(X509ContentType.Pfx));
  }

  private static void GetCaFilePaths(SelfSignedOptions options, out string caCrtPemFilePath, out string caKeyPemFilePath, out string caPfxFilePath)
  {
    caCrtPemFilePath = Path.Combine(options.CaFilePath, "ca.crt.pem");
    caKeyPemFilePath = Path.Combine(options.CaFilePath, "ca.key.pem");
    caPfxFilePath = Path.Combine(options.CaFilePath, "ca.pfx");
  }
}
