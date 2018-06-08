using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTAWeb
{
    public interface IWebMenuService
    {
        Task<List<WebMenuModel>> GetWebMenu();
        Task<List<WebMenuModel>> GetWebMenu(string userRole);
    }
}
