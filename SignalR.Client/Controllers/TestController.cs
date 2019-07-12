using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SignalR.Client.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Index1()
        {
            return View();
        }

        public IActionResult Index2()
        {
            return View();
        }

        public IActionResult Index3()
        {
            return View();
        }

        public ActionResult SendMessageToAll()
        {
            MySignalrHubClient.SignalrHub(new SignalrHubClient()
            {
                method = "SendMessageToAll",
                message = new { From = "00000000-0000-0000-0000-000000000000", Content = "SendMessageToAll" },
            });
            return Content("");
        }

        public ActionResult SendMessageToUser()
        {
            MySignalrHubClient.SignalrHub(new SignalrHubClient()
            {
                method = "SendMessageToUser",
                message = new { From = "00000000-0000-0000-0000-000000000000",To = "00000000-0000-0000-0000-000000000000", Content = "SendMessageTo1" },
            });
            return Content("");
        }

        public ActionResult SendMessageToGroup1()
        {
            MySignalrHubClient.SignalrHub(new SignalrHubClient()
            {
                method = "SendMessageToMyGroup",
                message = new { From = "00000000-0000-0000-0000-000000000000", Content = "SendMessageToGroup1", GroupId = "1"},
            });
            return Content("");
        }
        public ActionResult SendMessageToGroup1and2()
        {
            MySignalrHubClient.SignalrHub(new SignalrHubClient()
            {
                method = "SendMessageToGroups",
                message = new { From = "00000000-0000-0000-0000-000000000000", Content = "SendMessageToGroup1and2", ToGroupIds = new List<string> { "1", "2" } },
            });
            return Content("");
        }
    }
}