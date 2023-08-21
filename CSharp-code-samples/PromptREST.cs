using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace InWebo.ApiDemo
{
    public class PromptREST
    {

        private long serviceId = -1;

        private System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = null;

        const string PROMPT_OFFSET = "Offset";
        const string PROMPT_MAX_RESULTS = "MaxResults";
        
        const string PROMPT_QUERY_SORT = "\nSort result\n" + 
            "0: no sort\n" +
            "1: by login (ascending)\n" +
            "2: by login (descending)\n" +
            "3: by name (ascending)\n" +
            "4: by name (descending)\n" +
            "5: by mail (ascending)\n" +
            "6: by mail (descending)\n" +
            "(0 - 6)";
        const string PROMPT_LOGIN_ID = "Login Id";
        const string PROMPT_EXACT_MATCH = "Exact match (0=no, 1=yes)";
        const string PROMPT_LOGIN_NAME = "Login";

        const string PROMPT_FIRSTNAME = "Firstname";
        const string PROMPT_LASTNAME = "Lastname";
        const string PROMPT_MAIL = "Mail";
        const string PROMPT_PHONE = "Phone";
        const string PROMPT_STATUS = "Status (0=active, 1=inactive)";
        const string PROMPT_ROLE = "Role (0=user, 1=manager or 2=admin)";
        const string PROMPT_ACCESS = "Access (0=bookmarks or 1=no bookmarks)";
        const string PROMPT_CODE_TYPE = "Code type (0=15mn code, 1=3 week code or 2=3 week link)";
        const string PROMPT_LANG = "Lang (fr or en)";
        const string PROMPT_EXTRA_FIELDS = "Extra Fields";

        const string PROMPT_TOKEN = "Token";
        const string PROMPT_DATA = "Data to seal";
        const string PROMPT_IP = "IP";

        const string RESULT_OK = "OK";
        const string RESULT_NOK = "NOK";
        const string PROMPT_ERROR_FORMAT = "\nError: {0}";

        const int RET_OK = 1;
        const int RET_ERROR = -1;
        const int RET_QUIT = 0;

        private Commands menuAuthentication;

        public PromptREST(string p12File, string p12Password, long serviceId)
        {
            this.serviceId = serviceId;
            try {
                certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(@p12File, p12Password);
            } catch (SystemException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                throw (e);
            }
            initMenus();
        }


        private void initMenus()
        {
         
            menuAuthentication = new Commands();
            menuAuthentication.Add(new Command { Name = "1", Action = authenticateExtended, Description = "AuthenticateExtended" });
            menuAuthentication.Add(new Command { Name = "2", Action = sealVerify, Description = "Seal Verify" });
            menuAuthentication.Add(new Command { Name = "3", Action = pushAuthenticate, Description = "Push Authenticate" });
        }

        private string ask(string text, object defValue)
        {
            Console.Write("{0} [{1}]>", text, defValue);
            return Console.ReadLine();
        }

        private string askString(string text, string defValue)
        {
            string input = ask(text, defValue);
            if (input.Equals("")) {
                return defValue;
            } else {
                return input;
            }
        }

        private int askInt(string text, int defValue)
        {
            while (true) {
                string input = ask(text, defValue);
                int value;
                if (input.Equals("")) {
                    return defValue;
                } else {
                    try {
                        value = Int32.Parse(input);
                        return value;
                    } catch (ArgumentException) { }
                }
            }
        }

        private int authenticateExtended()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nAuthenticate Extended"); 
            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string token = askString(PROMPT_TOKEN, settings.token);
            settings.token = token;
            string url = "https://api.myinwebo.com/FS?" + "action=authenticateExtended"
                    + "&serviceId=" +  serviceId.ToString()
                    + "&userId=" + loginName
                    + "&token=" + token + "&format=json";

            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.ClientCertificates.Add(certificate);
            Request.Method = "GET";
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
            string s = sr.ReadLine();
            Console.WriteLine("\nResult: " +s); 
            return RET_OK;
        }

        private int sealVerify()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nSeal Verify");
            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string token = askString(PROMPT_TOKEN, settings.token);
            settings.token = token;

            string data = askString(PROMPT_DATA, "");
                        
            string url = "https://api.myinwebo.com/FS?" + "action=sealVerify"
                    + "&serviceId=" + serviceId.ToString()
                    + "&userId=" + loginName
                    + "&data=" + data
                    + "&token=" + token + "&format=json";
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.ClientCertificates.Add(certificate);
            Request.Method = "GET";
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
            string s = sr.ReadLine();
            Console.WriteLine("\nResult: " + s);
            return RET_OK;
        }

        private int pushAuthenticate()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nAsk Push Notification");
            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string url = "https://api.myinwebo.com/FS?" + "action=pushAuthenticate"
                    + "&serviceId=" + serviceId.ToString()
                    + "&userId=" + loginName + "&format=json";
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.ClientCertificates.Add(certificate);
            Request.Method = "GET";
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
            string s = sr.ReadLine();
            Console.WriteLine("\nResult: " + s);

            string sessionId = ""; //TODO: parse result to get sessionId
            while (true)
            {
                url = "https://api.myinwebo.com/FS?" + "action=checkPushResult"
                  + "&serviceId=" + serviceId.ToString()
                  + "&userId=" + loginName
                  + "&sessionId=" + sessionId + "&format=json";
                Request = (HttpWebRequest)WebRequest.Create(url);
                Request.ClientCertificates.Add(certificate);
                Request.Method = "GET";
                Response = (HttpWebResponse)Request.GetResponse();
                sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                s = sr.ReadLine();
                Console.WriteLine("\nCheck Result: " + s);
                break;
            }
            return RET_OK;
        }

        private int showMenuAuthentication()
        {
            showMenu(menuAuthentication);
            return RET_OK;
        }

        private int prompt(Commands menu, bool top = false)
        {
            Command cmd;

            Console.Write("\n\n{ ");
            foreach (Command c in menu) {
                Console.Write("{0}.{1} ; ", c.Name, c.Description);
            }
            Console.Write("0.{0}", top ? "quit " : "back ");
            Console.WriteLine(" }");
            Console.Write("cmd ]");
            string input = Console.ReadLine();
            if (input.Equals("0")) return RET_QUIT;
            try {
                cmd = menu[input];
            } catch (KeyNotFoundException) {
                return RET_ERROR;
            }
            return cmd.Action();
        }

        private void showMenu(Commands menu, bool top = false)
        {
            while (prompt(menuAuthentication, top) != RET_QUIT) ;
        }

        public void start()
        {
            Console.WriteLine("InWebo Api Demo C# prompt REST");
            showMenu(menuAuthentication, true);
        }
    }
}
