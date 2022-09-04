using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Utils {
    public class DirectoryUtils {

        /// <summary>
        /// Runtime generated string of the absolute path to the execution directory
        /// </summary>
        public static readonly string EXECUTION_DIRECTORY = Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        
        /// <summary>
        /// Runtime generated string of the absolute path to the code base directory
        /// Useful during development to reach the project root when debugging or executing from the IDE
        /// </summary>
        public static readonly string CODEBASE_ROOT_DIRECTORY = Path.GetFullPath(EXECUTION_DIRECTORY + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "..");




    }
}
