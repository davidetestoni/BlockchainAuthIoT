using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using BlockchainAuthIoT.Core.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class OCPManager
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public bool IsAdmin { get; set; }

        private OCPModel newOCP = new();

        private async Task CreateOCP()
        {
            try
            {
                await AccessControl.CreateOCP(newOCP);
                await js.AlertSuccess("Created new OCP");
                newOCP = new();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task GetOCPBoolParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPBoolParam(ocp, name);
                await js.AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task GetOCPIntParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPIntParam(ocp, name);
                await js.AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task GetOCPStringParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPStringParam(ocp, name);
                await js.AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task SetOCPBoolParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = bool.Parse(await js.GetPrompt("Enter the bool parameter value"));
                await AccessControl.SetOCPBoolParam(ocp, name, value);
                await js.AlertSuccess($"Added bool parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task SetOCPIntParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = int.Parse(await js.GetPrompt("Enter the int parameter value"));
                await AccessControl.SetOCPIntParam(ocp, name, value);
                await js.AlertSuccess($"Added int parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task SetOCPStringParam(OCP ocp)
        {
            try
            {
                var name = await js.GetPrompt("Enter the parameter name");
                var value = await js.GetPrompt("Enter the string parameter value");
                await AccessControl.SetOCPStringParam(ocp, name, value);
                await js.AlertSuccess($"Added string parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
