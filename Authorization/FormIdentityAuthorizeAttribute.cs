using Microsoft.AspNetCore.Authorization;

namespace TTAWeb
{
    // This attribute derives from the [Authorize] attribute, adding 
    // the ability for a user to specify a 'formName' paratmer. Since authorization
    // policies are looked up from the policy provider only by string, this
    // authorization attribute creates is policy name based on a constant prefix
    // and the user-supplied formName parameter. A custom authorization policy provider
    // (`FormIdentityPolicyProvider`) can then produce an authorization policy with 
    // the necessary requirements based on this policy name.
    internal class FormIdentityAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "FormIdentity";

        public FormIdentityAuthorizeAttribute(string formName)
        {
            FormName = formName;
        }

        // Get or set the FormName property by manipulating the underlying Policy property
        public string FormName
        {
            get
            {
                return default(string);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value}";
            }
        }
    }
}