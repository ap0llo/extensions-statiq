using System;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Enumerates Bootstrap table options.
    /// </summary>
    /// <seealso cref="UseBootstrapTables"/>
    /// <seealso href="https://getbootstrap.com/docs/4.5/content/tables/">Tables (Bootstrap Documentation)</seealso>
    [Flags]
    public enum BootstrapTableOptions
    {
        /// <summary>
        /// Use default Bootstrap table and only add the <c>table</c> class.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Add the <c>table-striped</c> class to all tables.
        /// </summary>
        Striped = 0x01,

        /// <summary>
        /// Add the <c>table-bordered</c> class to all tables.
        /// </summary>
        Bordered = 0x01 << 1,

        /// <summary>
        /// Add the <c>table-sm</c> class to all tables.
        /// </summary>
        Small = 0x01 << 2
    }
}
