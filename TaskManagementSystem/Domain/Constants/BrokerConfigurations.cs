namespace Domain.Constants
{
    public static class BrokerConfigurations
    {
        public static class QueueNames
        {
            public static readonly string TaskCreatedQueue = "task.created";
            public static readonly string TaskUpdatedQueue = "task.updated";
            public static readonly string CreateTaskQueue = "task.create";
            public static readonly string SearchTaskQueue = "task.search";
            public static readonly string TaskSearchResponseQueue = "task.search.responce";
        }
        
        public static class ExchangeNames
        {
            public static readonly string TaskExchange = "task.events";
        }
    }
}
