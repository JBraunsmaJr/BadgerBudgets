namespace BadgerBudgets.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Find the first item within <paramref name="collection"/> which fits criteria defined in <paramref name="search"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="search"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Index of item that matched condition. Otherwise, -1</returns>
    public static int Index<T>(this IEnumerable<T> collection, Predicate<T> search)
    {
        var index = 0;
        
        foreach (var item in collection)
        {
            if (search(item))
                return index;

            index++;
        }

        return -1;
    }
}