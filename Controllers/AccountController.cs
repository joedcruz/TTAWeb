using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace TTAWeb
{
    public class AccountController : Controller
    {
        string newToken;

        [HttpGet]
        public IActionResult Welcome(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string password, string returnUrl=null)
        {
            RegistrationInfo registrationInfo = new RegistrationInfo();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            registrationInfo.MobileNo = userName;
            registrationInfo.Password = password;
            var jsonData = new StringContent(JsonConvert.SerializeObject(registrationInfo), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/api/register", jsonData);
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;

            client.Dispose();

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        public IActionResult Signin(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signin(string userName, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(userName)) return BadRequest("A user name is required");
            if (string.IsNullOrEmpty(password)) return BadRequest("A password is required");

            // Call login api (authenticate using username and password, return token if successful)
            await GetToken(userName, password);

            
            // If the user login is successful
            string userId = "";
            string username = "";

            if (newToken != null)
            {
                // Call api GetUserInfo by passing the user token as header
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                HttpResponseMessage response = await client.GetAsync("http://localhost:5000/api/userinfo");
                response.EnsureSuccessStatusCode();
                var responseData = response.Content.ReadAsStringAsync();
                var userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData.Result.ToString());
                                
                foreach (var userInfo in userData)
                {
                    if (userInfo.Key == "userId")
                        userId = userInfo.Value;

                    if (userInfo.Key == "username")
                        username = userInfo.Value;
                }

                client.Dispose();
            }
                      

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, username));
            claims.Add(new Claim(ClaimTypes.Authentication, newToken));
           
            // Create user's identity and sign them in to create the Http Context
            var identity = new ClaimsIdentity(claims, "UserSpecified");
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            //return Redirect(returnUrl ?? "/");    

            return Redirect("/Account/Welcome");
        }


        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }


        public IActionResult Denied()
        {
            return View();
        }


        public async Task<string> GetToken(string userName, string password)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonData = new StringContent(JsonConvert.SerializeObject(new { MobileNo = userName, Password = password }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/api/login", jsonData);
            response.EnsureSuccessStatusCode();

            var responseData1 = response.Content.ReadAsStringAsync();
            newToken = JsonConvert.DeserializeObject(responseData1.Result).ToString();
            client.Dispose();

            return newToken;
        }
    }
}