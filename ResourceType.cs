using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyRes
{

    /// <summary>
    /// Tipo de recurso
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Archivo de imagen en commons.
        /// </summary>
        Image,
        /// <summary>
        /// Archivo de video en commons.
        /// </summary>
        Video,
        /// <summary>
        /// Archivo de audio en commons.
        /// </summary>
        Sound,
        /// <summary>
        /// Archivo de commons no reconocido.
        /// </summary>
        Other
    };

}
