using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    class General
    {
        public static void InitializateServer ()
        {
            Console.WriteLine("Iniciado TCP...");
            ServerTCP.intializeNetwork();
        }
    }
}
