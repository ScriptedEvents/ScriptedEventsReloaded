namespace SER.Code.Helpers.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Used to shut up the linter.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object that may be null.</param>
    /// <returns>The same object, or null if the object is null.</returns>
    // ReSharper disable once ReturnTypeCanBeNotNullable
    public static T? MaybeNull<T>(this T obj) where T : class
    {
        return obj;
    }

    public static string FriendlyTypeName(this object obj)
    {
        return obj.GetType().FriendlyTypeName();
    }

    public static T WithCurrent<T>(this T obj, Func<T, T> func) => func(obj);
}