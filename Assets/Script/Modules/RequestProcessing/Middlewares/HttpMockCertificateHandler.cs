using UnityEngine.Networking;

namespace Modules.RequestProcessing.Middlewares
{
    public class HttpMockCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
            =>  true;
    }
}