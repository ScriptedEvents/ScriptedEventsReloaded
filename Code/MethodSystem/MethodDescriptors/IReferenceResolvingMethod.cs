using SER.Code.Extensions;

namespace SER.Code.MethodSystem.MethodDescriptors;

/// <summary>
/// A method which takes a reference as an input and returns information about it in a readable format.
/// </summary>
public interface IReferenceResolvingMethod
{
    public Type ResolvesReference { get; }

    public static class Desc
    {
        public static string Get(IReferenceResolvingMethod method) => $"Extracts information from {method.ResolvesReference.AccurateName} objects.";
    }
}