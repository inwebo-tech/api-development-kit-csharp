using System;
using System.Collections.Generic;
using System.Text;

namespace InWebo.ApiDemo
{
    class Program
    {
        private static String p12file = "C:\\Users\\Emmanuel\\Desktop\\iwds_conf_default\\TEST.p12"; // Specify here the name of your certificate file.
        private static String p12password = "iwdstest"; // This is the password to access your certificate file
        private static long serviceId = 80; // This is the id of your service.

        static void Main(string[] args)
        {
            //PromptREST prompt = new PromptREST(p12file, p12password, serviceId);
            Prompt prompt = new Prompt(p12file, p12password, serviceId);
            prompt.start();
            Properties.Settings.Default.Save();
        }
    }
}
