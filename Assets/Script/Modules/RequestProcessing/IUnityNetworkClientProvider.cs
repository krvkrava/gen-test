using Strx.Expansions.Modules.ModulesManagement.Attributes;
using Strx.Expansions.Modules.RequestProcessing.Abstractions;

namespace Modules.RequestProcessing
{
    [ModuleProvider]
    public interface IUnityNetworkClientProvider : INetworkClientProvider
    {
        
    }
}