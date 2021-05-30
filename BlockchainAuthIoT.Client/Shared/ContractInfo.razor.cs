using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ContractInfo
    {
        [Inject] private AccessControlService AccessControl { get; set; }
    }
}
