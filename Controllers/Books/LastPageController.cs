using Microsoft.AspNetCore.Mvc;

namespace BookInfo.Controllers;

public class LastPageController : Controller
{
    public void AddLastPage(string page)
    {
        string? lastpages = TempData["lastpage"] as string;

        void updateTempdata()
        {
            TempData["lastpage"] = lastpages + "-" + page;
        }

        string[] lastpagesArray = lastpages.Split("-");
        if (lastpages == null)
        {
            TempData["lastpage"] = page;
        }
        else
        {
            if (page.StartsWith("IndexBook"))
            {
                if (!lastpages.Contains("IndexBook"))
                {
                    updateTempdata();
                }
                else if (lastpagesArray[^2].StartsWith("IndexBook"))
                {
                    lastpagesArray[^2] = lastpagesArray[^1];
                    lastpagesArray[^1] = page;
                    TempData["lastpage"] = string.Join('-', lastpagesArray);
                }
            }
            else if (page.StartsWith("IndexCategory"))
            {
                if (!lastpages.Contains("IndexCategory"))
                {
                    updateTempdata();
                }
                else if (lastpagesArray[^1].StartsWith("IndexBook"))
                {
                    lastpagesArray[^2] = lastpagesArray[^1];
                    lastpagesArray[^1] = page;
                    TempData["lastpage"] = string.Join('-', lastpagesArray);
                }
            }
            else if (!lastpages.Contains(page))
            {
                updateTempdata();
            }
        }
    }


    public string? RemoveLastPage(bool keepPage)
    {
        string? lastpages = TempData["lastpage"] as string;

        if (lastpages == null)
        {
            TempData["lastpage"] = null;
            return null;
        }
        else
        {
            string[] lastpagesArray = lastpages.Split("-");
            string value = lastpagesArray[^1];
            if (!keepPage)
            {
                TempData["lastpage"] = string.Join('-', lastpagesArray.SkipLast(1).ToArray());
            }
            return value;
        }
    }



    private static int getId(string lastpage)
    {
        return Convert.ToInt32(lastpage.Split("_")[1]);
    }

    public IActionResult Return(string currentpage = "", bool? returnToPage = false)
    {
        bool keepPage;
        if (returnToPage == null)
        {
            keepPage = false;
        }
        else
        {
            keepPage = (bool)returnToPage;
        }
        string? lastPage = RemoveLastPage(keepPage);
        if (currentpage == lastPage) lastPage = RemoveLastPage(keepPage);
        if (lastPage != null)
        {
            if (lastPage == "Home")
            {
                return RedirectToAction("Index", "Home");
            }
            else if (lastPage == "CategoryList")
            {
                return RedirectToAction("List", "Category");
            }
            else if (lastPage == "CreateCategory")
            {
                return RedirectToAction("Create", "Category");
            }
            else if (lastPage == "AuthorList")
            {
                return RedirectToAction("List", "Author");
            }
            else if (lastPage == "CreateAuthor")
            {
                return RedirectToAction("Create", "Author");
            }
            else if (lastPage == "PublisherList")
            {
                return RedirectToAction("List", "Publisher");
            }
            else if (lastPage == "CreatePublisher")
            {
                return RedirectToAction("Create", "Publisher");
            }
            else if (lastPage == "BookList")
            {
                return RedirectToAction("List", "Book");
            }
            else if (lastPage == "CreateBook")
            {
                return RedirectToAction("Create", "Book");
            }
            else if (lastPage.StartsWith("EditBook"))
            {
                int id = getId(lastPage);
                return RedirectToAction("Edit", "Book", new { id = id });
            }
            else if (lastPage.StartsWith("IndexBook"))
            {
                int id = getId(lastPage);
                return RedirectToAction("Index", "Book", new { id = id });
            }
            else if (lastPage.StartsWith("IndexCategory"))
            {
                int id = getId(lastPage);
                return RedirectToAction("Index", "Category", new { id = id });
            }
            else
            {
                return RedirectToAction("Index", "Home");

            }
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Cancel(string? message, string? currentpage)
    {
        if (message != null) TempData["info"] = message;
        return Return(currentpage);
    }
}