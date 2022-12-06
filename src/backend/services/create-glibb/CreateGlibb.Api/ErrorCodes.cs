namespace CreateGlibb.Api;

public static class ErrorCodes
{
    private static class CommonCodes
    {
        public const string CantBeNull = "cant_be_null";
        public const string CantBeEmpty = "cant_be_empty";
        public const string CantBeLongerThanSpecification = "cant_be_longer_than_specification";
    }

    public static class Glibb
    {
        private const string Subject = "glibb";
        
        public static class Message
        {
            private const string Property = $"{Subject}.Message";
            
            public const string CantBeNull = $"{Property}.{CommonCodes.CantBeNull}";
            public const string CantBeEmpty = $"{Property}.{CommonCodes.CantBeEmpty}";
            public static string CantBeLongerThanSpecification = $"{Property}.{CommonCodes.CantBeLongerThanSpecification}";
        }
    }
}