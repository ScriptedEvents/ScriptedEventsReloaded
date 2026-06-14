namespace SER.Code.Extensions;

public static class EnumExtensions
{
    extension<T>(T enumValue) where T : struct, Enum
    {
        public EnumValue<T> ToEnumValue()
        {
            return new EnumValue<T>(enumValue);
        }
        
        public IEnumerable<T> GetFlags()
        {
            return from T flag in Enum.GetValues(typeof(T)) 
                where Convert.ToUInt64(enumValue) != 0
                where Convert.ToUInt64(flag) != 0 
                where enumValue.HasFlag(flag) 
                select flag;
        }
    }

}
