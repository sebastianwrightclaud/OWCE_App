using System;
using System.Threading.Tasks;
using OWCE.DependencyInterfaces;


namespace OWCE.Platforms.iOS.DependencyImplementations
{
    public class PermissionPrompt : IPermissionPrompt
    {
        public async Task<bool> PromptBLEPermission()
        {
            return true;
        }
    }
}
