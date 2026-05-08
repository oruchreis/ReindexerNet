using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Configures diagnostic endpoints and allocation diagnostics for the embedded Reindexer server.
    /// </summary>
    public class DebugOptions
    {
        /// <summary>
        /// Enables the pprof diagnostic endpoint.
        /// </summary>
        public bool PProf { get; set; } = false;
        /// <summary>
        /// Enables allocation diagnostics.
        /// </summary>
        public bool Allocs { get; set; } = false;
    }
}
