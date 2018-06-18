using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace TTAWeb
{
    public class WebMenuService : IWebMenuService
    {
        private readonly IHttpContextAccessor _accessor; // Dependency injection to access the Http context
        string userId;
        string userToken;
        string _menuString;


        public WebMenuService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }


        //public async Task<List<WebMenuModel>> GetWebMenu(string userRole)
        //{
        //    HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage response = await client.GetAsync("http://localhost:5000/api/getwebusermenu");
        //    response.EnsureSuccessStatusCode();

        //    var responseData1 = response.Content.ReadAsStringAsync().Result;
        //    List<WebMenuModel> menu = JsonConvert.DeserializeObject<List<WebMenuModel>>(responseData1);
        //    client.Dispose();

        //    return menu;
        //}


        //public async Task<List<WebMenuModel>> GetWebMenu()
        public async Task<string> GetWebMenu()
        {
            if (!string.IsNullOrEmpty(_accessor.HttpContext.User.Identity.Name))
            {
                var contextClaims = _accessor.HttpContext.User.Claims;

                // Extract UserId from User Claims
                userId = (contextClaims.SingleOrDefault(val => val.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")).Value;
                // Extract User token from User Claims
                userToken = (contextClaims.SingleOrDefault(val => val.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication")).Value;
            }

            UserInfoModel _userInfoModel = new UserInfoModel();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            _userInfoModel.UserId = userId;
            //_userInfoModel.UserId = "ef5be01f-cc0d-4e69-b819-b77f8b2d94f3"; //user 1
            //_userInfoModel.UserId = "ecb0bd58-1b77-4db8-b291-d0c9a1fd10fe"; //user 2
            //_userInfoModel.UserId = "7c02fc06-cee9-4a14-937b-35d26dadd54d"; //user 3
            var jsonData = new StringContent(JsonConvert.SerializeObject(_userInfoModel), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/api/getwebusermenu", jsonData);
            response.EnsureSuccessStatusCode();

            var responseData = response.Content.ReadAsStringAsync().Result;;
            //List<WebMenuModel> menu = JsonConvert.DeserializeObject<List<WebMenuModel>>(responseData);
            List<WebMenuModel> menu = JsonConvert.DeserializeObject<List<WebMenuModel>>(responseData).OrderBy(ord => ord.ItemId).ToList();
            //string menu = JsonConvert.DeserializeObject(responseData).ToString();
            _menuString = "";
            BuildMenu(menu);

            client.Dispose();

            return _menuString;
            //return menu;
        }

        public void BuildMenu(List<WebMenuModel> fullMenu)
        {
            _menuString = "<ul class='nav navbar-nav'>";

            foreach (var menu in fullMenu)
            {
                if (menu.ParentId == 0)
                {
                    SubMenu(fullMenu, menu);
                }
            }
            _menuString = _menuString + "</ul>";
        }

        public void SubMenu(List<WebMenuModel> fullMenu, WebMenuModel menu)
        {
            //_menuList = _menuList + "<li [routerLinkActive]=\"['link-active']\">< a[routerLink] = \"['/home']\" >< span class='glyphicon glyphicon-home'></span>" + page.DisplayName + "</a>";
            _menuString = _menuString + "<li><a><span class='glyphicon glyphicon-home'></span>" + menu.DisplayName + "</a>";

            var subMenus = fullMenu.Where(p => p.ParentId == menu.ItemId);

            if (subMenus.Count() > 0)
            {
                _menuString = _menuString + "<ul class='nav navbar-nav'>";

                foreach (WebMenuModel p in subMenus)
                {
                    if (fullMenu.Count(x => x.ParentId == p.ItemId) > 0)
                    {
                        SubMenu(fullMenu, p);
                    }
                    else
                    {
                        //_menuList = _menuList + "<li [routerLinkActive]=\"['link-active']\">< a[routerLink] = \"['/home']\" >< span class='glyphicon glyphicon-th-list'></span>" + p.DisplayName + "</a></li>";
                        _menuString = _menuString + "<li><a><span class='glyphicon glyphicon-th-list'></span>" + p.DisplayName + "</a></li>";
                    }
                }
                _menuString = _menuString + "</ul>";
            }
            _menuString = _menuString + "</li>";
        }
    }
}
