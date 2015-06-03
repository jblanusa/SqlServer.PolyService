namespace SqlServer.PolyService
{
    /// <summary>
    /// This should be interface for Azure Services.
    /// Azure Service contains some key and version
    /// that is going to be used
    /// </summary>
    interface IAzureService
    {
        string Key
        {
            get;
            set;
        }

        string Version
        {
            get;
            set;
        }
    }
}
