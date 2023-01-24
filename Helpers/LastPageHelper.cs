namespace BookInfo.Helpers;
using System.Text.RegularExpressions;

public class LastPages
{
    // Add page ID to the last page array
    public static string AddLastPage(string lastPages, string pageId)
    {
        void addItem()
        {
            lastPages += "-" + pageId;
        }

        string[] stackArray = lastPages.Split("-");

        // If it is an index page
        if (pageId.StartsWith("Index"))
        {
            // Index page format: IndexType_ID, e.g IndexBook_19
            string lastPageType = pageId.Split('_')[0];

            // C# syntax for accessing last item in array
            string lastItem = stackArray[^1];

            if (!pageId.Contains(lastPageType))
            {
                addItem();
            }
            // If the second last page is an index and it isn't of the same type
            else if (lastItem.StartsWith("Index") && !lastItem.StartsWith(lastPageType))
            {
                stackArray[^2] = stackArray[^1];
                stackArray[^1] = pageId;
                return string.Join('-', stackArray);
            }
        }
        else if (!lastPages.Contains(pageId)) addItem();

        return lastPages;
    }

    // Remove the last value of the lastpage temp data, by splitting it into an array and removing the value
    private static (string[] lastPages, string value) RemoveLastPage(string lastPages, bool keepPage)
    {
        string[] lastPagesArray = lastPages.Split("-");
        string value = lastPagesArray[^1];
        // If we decide not to keep the page
        if (!keepPage)
        {
            lastPages = string.Join('-', lastPagesArray.SkipLast(1).ToArray());
        }

        return (lastPagesArray, value);
    }


    // Return the lastpage stack, controller, action and ID after popping value from lastpage stack
    public (string lastPages, string controller, string action, int id) Return(string lastPages, string currentPage = "", bool? keepPage = false)
    {
        // Get the last page and remove it (optional)
        var lastPage = RemoveLastPage(lastPages, keepPage ?? false).value;

        // If the current page is equal to the last page, then remove the last page again
        if (currentPage == lastPage) lastPage = RemoveLastPage(lastPages, keepPage ?? false).value;

        if (lastPage == null) return (lastPages, "Index", "Home", 0);

        // Format for lastpages: ControllerAction_Id 
        //                                       ^ 
        //                                    optional

        // Split the lastpageId by capital letters
        var lastPageSplit = Regex.Split(lastPage, "(?<!^)(?=[A-Z])");

        // Get controller and action
        string controller = lastPageSplit[0];
        string action = lastPageSplit[1];
        int id = 0;

        // If contains ID, extract id, and then set action to action without ID
        if (action.Contains('_'))
        {
            var actionSplit = Regex.Split(action, "_");
            action = actionSplit[0];
            id = Convert.ToInt32(actionSplit[1]);
        }
        return (lastPages, controller, action, id);
    }
}