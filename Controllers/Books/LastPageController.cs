using Microsoft.AspNetCore.Mvc;

namespace BookInfo.Controllers;

/*

lastpage tempdata format:

page1-page2-page3_value

Data is spread by hyphens
Values are spread by underscores

*/

public class LastPageController : Controller
{
    // Public method to add a last page to the last pages tempdata
    public void AddLastPage(string pageToAdd)
    {
        string? lastpages = TempData["lastpage"] as string;

        // Add the page to the end of the string
        void addLastPage()
        {
            TempData["lastpage"] = lastpages + "-" + pageToAdd;
        }

        // If there is currently no lastpages, then just put the page to add
        if (lastpages == null)
        {
            TempData["lastpage"] = pageToAdd;
        }
        else
        {
            // Split it into an array 
            string[] lastpagesArray = lastpages.Split("-");


            if (pageToAdd.StartsWith("Index"))
            {
                // Format for index pages: IndexType_IndexId, e.g IndexBook_19
                string[] lastpageIndex = pageToAdd.Split("_");

                    string secondLastPage = lastpagesArray[^1];

                // If the current index type isn't in there, then just add it
                if (!lastpages.Contains(lastpageIndex[0]))
                {
                    addLastPage();
                }
                // Else, if the second last value is an index, and is not the same type, then swap the values
                else if (secondLastPage.StartsWith("Index")
                && !secondLastPage.StartsWith(lastpageIndex[0]))
                {
                    lastpagesArray[^2] = lastpagesArray[^1];
                    lastpagesArray[^1] = pageToAdd;
                    TempData["lastpage"] = string.Join('-', lastpagesArray);
                }
            }
            // Else if it doesn't exist then just add it
            else if (!lastpages.Contains(pageToAdd))
            {
                addLastPage();
            }
        }
    }

    // Remove the last value of the lastpage temp data, by splitting it into an array and removing the value
    public string? RemoveLastPage(bool keepPage)
    {
        string? lastpages = TempData["lastpage"] as string;

        if (lastpages == null)
        {
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


    // Get the value from a lastpage value by splitting it into an array
    private static int getValue(string lastpage)
    {
        return Convert.ToInt32(lastpage.Split("_")[1]);
    }

    // Return to the last page
    public IActionResult Return(string currentpage = "", bool? returnToPage = false)
    {
        bool keepPage;

        /* If returnToPage is true, then it won't remove the last value, so will return the current page */

        if (returnToPage == null)
        {
            keepPage = false;
        }
        else
        {
            keepPage = (bool)returnToPage;
        }

        // Get the last page and remove it
        string? lastPage = RemoveLastPage(keepPage);

        // If the current page is equal to the last page, then remove the last page again
        if (currentpage == lastPage) lastPage = RemoveLastPage(keepPage);

        // Redirect
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
                int id = getValue(lastPage);
                return RedirectToAction("Edit", "Book", new { id = id });
            }
            else if (lastPage.StartsWith("IndexBook"))
            {
                int id = getValue(lastPage);
                return RedirectToAction("Index", "Book", new { id = id });
            }
            else if (lastPage.StartsWith("IndexCategory"))
            {
                int id = getValue(lastPage);
                return RedirectToAction("Index", "Category", new { id = id });
            }
            else if (lastPage.StartsWith("IndexAuthor"))
            {
                int id = getValue(lastPage);
                return RedirectToAction("Index", "Author", new { id = id });
            }
            else if (lastPage.StartsWith("IndexPublisher"))
            {
                int id = getValue(lastPage);
                return RedirectToAction("Index", "Publisher", new { id = id });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    // Send a notification and return
    public IActionResult Cancel(string? message, string? currentpage)
    {
        if (message != null) TempData["info"] = message;
        return Return(currentpage);
    }
}