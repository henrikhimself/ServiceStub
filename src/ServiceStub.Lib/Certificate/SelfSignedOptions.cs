namespace Hj.ServiceStub.Certificate;

public sealed class SelfSignedOptions
{
  public required string FilePath { get; set; }

  public required string AlgorithmOid { get; set; }

  public required string SubjectName { get; set; }

  public required string[] DnsNames { get; set; }
}
