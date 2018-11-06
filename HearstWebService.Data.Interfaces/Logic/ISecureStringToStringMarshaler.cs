using System;

namespace HearstWebService.Interfaces
{
    public interface ISecureStringToStringMarshaller : IDisposable
    {
        string InsecureString { get; }
    }
}
