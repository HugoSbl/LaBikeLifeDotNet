using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LaBikeLifeDotNet.Models;
using LaBikeLifeDotNet.Services;
using LaBikeLifeDotNet.ViewModels;

namespace LaBikeLifeDotNet.Controllers;

public class HomeController(INhtsaVpicService vpic) : Controller
{
    public async Task<IActionResult> Index()
    {
        // on n'affiche la recherche que si la personne est connectée
        if (User.Identity?.IsAuthenticated == true)
        {
            var makes = await vpic.GetMotorcycleMakesAsync();
            var currentYear = DateTime.UtcNow.Year;
            var years = Enumerable.Range(1995, currentYear - 1995 + 1).Reverse().ToList();

            return View(new MotorcycleSearchViewModel { Makes = makes, Years = years });
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
