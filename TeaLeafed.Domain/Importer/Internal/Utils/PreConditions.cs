using System;

namespace TeaLeafed.Domain.Importer.Internal.Utils
{
    /// <summary>
    /// Class used to Make sure input args are not null
    /// </summary>
    internal static class PreConditions
    {
        public static T NotNull<T>(this T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            return value;
        }
    }
}
