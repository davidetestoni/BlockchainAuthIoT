using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Signer
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Inject] private IAccountProvider AccountProvider { get; set; }
    }
}
