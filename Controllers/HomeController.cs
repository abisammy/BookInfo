﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;

namespace BookInfo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        TempData["lastpage"] = "Home";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Cookie()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
