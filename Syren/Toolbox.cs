using System.Text;

namespace Syren.Syren;

public class Toolbox
{
    public static List<T>  Shuffle<T>(List<T> list)  
    {  
        var rng = new Random();  
        var n = list.Count;  
        while (n > 1) {  
            n--;  
            var k = rng.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }
    
    public static (string, int, int) CreatePageFromList(List<string> stringList, string pageQuery, bool sort, int length, bool enumerate)
    {
        var pages = new List<string>();
        var stringBuilder = new StringBuilder();
        if (sort) {stringList.Sort();}
        
        int pagenumber;
        if (string.IsNullOrWhiteSpace(pageQuery))
        {
            pagenumber = 1;
        }
        else
        {
            var isNumeric = int.TryParse(pageQuery, out pagenumber);
            pagenumber = isNumeric && pagenumber >= 1 ? pagenumber : 1;
        }

        var index = 1;
        foreach (var line in stringList)
        {
            stringBuilder.Append((enumerate ? $"{index}: " : "") + $"{line}\n");
            if (stringBuilder.Length > length)
            {
                pages.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }

            index++;
        }
        if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()))
        {
            pages.Add(stringBuilder.ToString());
        }
        
        if (pagenumber > pages.Count)
        {
            pagenumber = pages.Count;
        }
        return (pages[pagenumber - 1], pages.Count, pagenumber);
    }
}