using System;

namespace Server.IdUtil
{
    public static class UniqueIdGenerator
    {
        public static string GetGUID()
        {
            return new Guid().ToString();
        }
    }
}