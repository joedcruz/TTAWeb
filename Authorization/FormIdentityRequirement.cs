using Microsoft.AspNetCore.Authorization;

namespace TTAWeb
{
    internal class FormIdentityRequirement : IAuthorizationRequirement
    {
        public string FormName { get; private set; }

        public FormIdentityRequirement(string formName) { FormName = formName; }
    }
}