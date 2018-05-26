using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TTAWeb
{
    // This class contains logic for determining whether FormIdentityRequirements in authorizaiton
    // policies are satisfied or not
    internal class FormIdentityAuthorizationHandler : AuthorizationHandler<FormIdentityRequirement>
    {
        private readonly ILogger<FormIdentityAuthorizationHandler> _logger;
        private readonly IHttpContextAccessor _accessor; // Dependency injection to access the Http context

        string userId; // Stores the userId from the context
        List<string> userRoles = null; // Stores the user roles retrieved using the API GetUserRoles
        List<string> roleClaims = null; // Stores the role claims retrieved uding the API GetRoleClaims
        string userToken; // Stores the user token from the context

        public FormIdentityAuthorizationHandler(ILogger<FormIdentityAuthorizationHandler> logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }

        // Check whether a given FormIdentityRequirement is satisfied or not for a particular context
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, FormIdentityRequirement requirement)
        {
            // Log as a warning so that it's very clear in sample output which authorization policies 
            // (and requirements/handlers) are in use
            _logger.LogWarning("Evaluating authorization requirement for forms");

            // Retrieve all user claims from the Http context
            var contextClaims = _accessor.HttpContext.User.Claims;

            if (!string.IsNullOrEmpty(_accessor.HttpContext.User.Identity.Name))
            {
                // Extract UserId from User Claims
                userId = (contextClaims.SingleOrDefault(val => val.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")).Value;
                // Extract User token from User Claims
                userToken = (contextClaims.SingleOrDefault(val => val.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication")).Value;
            }

            // Retrieve all the roles associated with this user
            await GetUserRoles(userToken);
            
            // Retrieve all the roles who has given access to the form
            await GetRoleClaims(requirement.FormName, userToken);
            
            bool roleMatch = false;

            // Check if any roles match
            foreach(var x in userRoles)
            {
                foreach(var y in roleClaims)
                {
                    if (x == y)
                    {
                        roleMatch = true;
                        break;
                    }
                }
            }

            // If any role match, then authorize
            if (roleMatch == true)
            {
                _logger.LogInformation("Form Identity authorization requirement satisfied");
                context.Succeed(requirement);
            }                      

            return;
        }


        // Api method for retrieving all roles assigned to the user
        public async Task<List<string>> GetUserRoles(string token)
        {
            if (userId != null)
            {
                // Call api GetUserRoles by passing the user id as body and user token as header
                UserInfoModel _userInfoModel = new UserInfoModel();

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _userInfoModel.UserId = userId;
                var jsonData = new StringContent(JsonConvert.SerializeObject(_userInfoModel), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/api/userroles", jsonData);
                response.EnsureSuccessStatusCode();
                var responseData = response.Content.ReadAsStringAsync().Result;
                userRoles = JsonConvert.DeserializeObject<List<string>>(responseData.ToString());

                client.Dispose();
            }

            return userRoles;
        }


        // Api method for retrieving all roles who has access to the form
        public async Task<List<string>> GetRoleClaims(string formName, string token)
        {
            if (userRoles != null)
            {
                // Call api GetRoleClaims by passing the claim type and claim value as body and user token as header
                RoleClaimsModel _rolesClaimsModel = new RoleClaimsModel();

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _rolesClaimsModel.Type = "Form";
                _rolesClaimsModel.Value = formName;
                var jsonData = new StringContent(JsonConvert.SerializeObject(_rolesClaimsModel), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://localhost:5000/api/roleclaims", jsonData);
                response.EnsureSuccessStatusCode();
                var responseData4 = response.Content.ReadAsStringAsync().Result;
                roleClaims = JsonConvert.DeserializeObject<List<string>>(responseData4.ToString());

                client.Dispose();                
            }

            return roleClaims;
        }
    }
}