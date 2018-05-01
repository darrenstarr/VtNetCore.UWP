namespace VtNetCore.UWP.App.ViewModel
{
    public enum EScope
    {
        /// <summary>
        /// Scoped to the machine and user
        /// </summary>
        Global,

        /// <summary>
        /// Scoped to the tenant
        /// </summary>
        Tenant,

        /// <summary>
        /// Scoped to the site
        /// </summary>
        Site,

        /// <summary>
        /// Scoped to the device
        /// </summary>
        Device,
    }
}
