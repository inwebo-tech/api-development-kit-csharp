using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace InWebo.ApiDemo
{
    using Provisioning;
    using Authentication;

    public class Prompt
    {

        private long serviceId = -1;

        private System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = null;

        private ConsoleAdminService _adminWS = null;
        
        private ConsoleAdminService adminWS
        {
            get
            {
                if (_adminWS == null) {
                    _adminWS = new ConsoleAdminService();                  
                    if (certificate != null) {
                        _adminWS.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        _adminWS.ClientCertificates.Add(certificate);
                    }
                }
                return _adminWS;
            }
            set
            {
                _adminWS = value;
            }
        }

        private AuthenticationService _authWS = null;

        private AuthenticationService authWS
        {
            get
            {
                if (_authWS == null) {
                    _authWS = new AuthenticationService();
                    if (certificate != null) {
                        _authWS.ClientCertificates.Add(certificate);
                    }
                }
                return _authWS;
            }
            set
            {
                _authWS = value;
            }
        }

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
        const string PROMPT_IP = "IP";

        const string RESULT_OK = "OK";
        const string RESULT_NOK = "NOK";
        const string PROMPT_ERROR_FORMAT = "\nError: {0}";

        const int RET_OK = 1;
        const int RET_ERROR = -1;
        const int RET_QUIT = 0;

        private Commands menuTop;
        private Commands menuProvisioning;
        private Commands menuAuthentication;
        private Commands menuProvExt;

        public Prompt(string p12File, string p12Password, long serviceId)
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
            menuTop = new Commands();
            menuTop.Add(new Command { Name = "1", Description = "Provisioning", Action = showMenuProvisioning });
            menuTop.Add(new Command { Name = "2", Description = "Prov Other Commands", Action = showMenuProvExt });
            menuTop.Add(new Command { Name = "3", Description = "Authentication", Action = showMenuAuthentication });
            
            menuProvisioning = new Commands();
            menuProvisioning.Add(new Command { Name = "1", Action = loginsQuery, Description = "LoginsQuery" });
            menuProvisioning.Add(new Command { Name = "2", Action = loginQuery, Description = "LoginQuery" });
            menuProvisioning.Add(new Command { Name = "3", Action = loginSearch, Description = "LoginSearch" });
            menuProvisioning.Add(new Command { Name = "4", Action = loginCreate, Description = "LoginCreate" });
            menuProvisioning.Add(new Command { Name = "5", Action = loginUpdate, Description = "LoginUpdate" });
            menuProvisioning.Add(new Command { Name = "6", Action = loginSendByMail, Description = "LoginSendByMail" });
            menuProvisioning.Add(new Command { Name = "7", Action = loginActivateCode, Description = "LoginActivateCode" });
            menuProvisioning.Add(new Command { Name = "8", Action = loginGetActivationCodeFromLink, Description = "LoginGetActivationCodeFromLink" });
            menuProvisioning.Add(new Command { Name = "9", Action = loginDelete, Description = "LoginDelete" });

            menuProvExt = new Commands();
            menuProvExt.Add(new Command { Name = "1", Action = loginAddDevice, Description = "LoginAddDevice" });
            menuProvExt.Add(new Command { Name = "2", Action = loginResetPINErrorCounter, Description = "LoginResetPINErrorCounter" });
            menuProvExt.Add(new Command { Name = "3", Action = loginResetPwdExtended, Description = "LoginResetPwd" });
            menuProvExt.Add(new Command { Name = "4", Action = loginRestore, Description = "LoginRestore" });

            menuAuthentication = new Commands();
            menuAuthentication.Add(new Command { Name = "1", Action = authenticate, Description = "Authenticate" });
            menuAuthentication.Add(new Command { Name = "2", Action = authenticateWithIp, Description = "Authenticate with IP" });

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

        private int loginsQuery()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nQuery list of logins");
            int offset = askInt(PROMPT_OFFSET, settings.offset);
            settings.offset = offset;
            int maxResults = askInt(PROMPT_MAX_RESULTS, settings.maxResults);
            settings.maxResults = maxResults;
            int querySort = askInt(PROMPT_QUERY_SORT, settings.querySort);
            settings.querySort = querySort;

            try {
                LoginsQueryResult queryResult = adminWS.loginsQuery(0, serviceId, offset, maxResults, querySort);
                if (!queryResult.err.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult.err);
                    return RET_ERROR;
                }
                Console.Write("\nGot {0} users from a total of {1}", queryResult.n, queryResult.count);
                UserList users = UserList.FromLoginsResult(queryResult);
                users.DisplayInfo();
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginQuery()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nQuery one login");
            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);            
            settings.loginId = loginId;

            try {
                LoginQueryResult queryResult = adminWS.loginQuery(0, loginId);
                if (!queryResult.err.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult.err);
                    // No return here since loginQuery always returns "NOK"
                    //return 0;

                }
                User user = User.FromLoginResult(queryResult);
                user.DisplayInfo();
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginSearch()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nSearch logins");
            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;
            int exactMatch = askInt(PROMPT_EXACT_MATCH, settings.exactMatch);
            settings.exactMatch = exactMatch;
            int offset = askInt(PROMPT_OFFSET, settings.offset);
            settings.offset = offset;
            int maxResults = askInt(PROMPT_MAX_RESULTS, settings.maxResults);
            settings.maxResults = maxResults;
            int querySort = askInt(PROMPT_QUERY_SORT, settings.querySort);
            settings.querySort = querySort;

            try {
                LoginSearchResult queryResult = adminWS.loginSearch(0, serviceId, loginName, exactMatch, offset, maxResults, querySort);
                if (!queryResult.err.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult.err);
                    return RET_ERROR;
                }
                UserList users = UserList.FromLoginsResult(queryResult);
                Console.Write("\nFound {0} users", queryResult.n);
                users.DisplayInfo();
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginCreate()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nCreate login");

            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string firstname = askString(PROMPT_FIRSTNAME, settings.firstname);
            settings.firstname = firstname;

            string lastname = askString(PROMPT_LASTNAME, settings.lastname);
            settings.lastname = lastname;

            string mail = askString(PROMPT_MAIL, settings.mail);
            settings.mail = mail;

            string phone = askString(PROMPT_PHONE, settings.phone);
            settings.phone = phone;

            int status = askInt(PROMPT_STATUS, settings.status);
            settings.status = status;

            int role = askInt(PROMPT_ROLE, settings.role);
            settings.role = role;

            int access = askInt(PROMPT_ACCESS, settings.access);
            settings.access = access;

            int codeType = askInt(PROMPT_CODE_TYPE, settings.codeType);
            settings.codeType = codeType;

            string lang = askString(PROMPT_LANG, settings.lang);
            settings.lang = lang;

            string extraFields = askString(PROMPT_EXTRA_FIELDS, settings.extraFields);
            settings.extraFields = extraFields;

            try
            {
                LoginCreateResult queryResult = adminWS.loginCreate(0, serviceId, loginName, firstname, lastname, mail, phone, status, role, access, codeType, lang, extraFields);
                if (!queryResult.err.Equals(RESULT_OK))
                {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult.err);
                    return RET_ERROR;
                }
                Console.Write("\nCode: {0}", queryResult.code);
                Console.Write("\nId: {0}", queryResult.id);
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginGetActivationCodeFromLink()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nCreate login");

            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string firstname = askString(PROMPT_FIRSTNAME, settings.firstname);
            settings.firstname = firstname;

            string lastname = askString(PROMPT_LASTNAME, settings.lastname);
            settings.lastname = lastname;

            string mail = askString(PROMPT_MAIL, settings.mail);
            settings.mail = mail;

            string phone = askString(PROMPT_PHONE, settings.phone);
            settings.phone = phone;

            int status = askInt(PROMPT_STATUS, settings.status);
            settings.status = status;

            int role = askInt(PROMPT_ROLE, settings.role);
            settings.role = role;

            int access = askInt(PROMPT_ACCESS, settings.access);
            settings.access = access;

            Console.WriteLine("Codetype arbitrarily set to 2 (generate a 3 week long code)");
            settings.codeType = 2;

            string lang = askString(PROMPT_LANG, settings.lang);
            settings.lang = lang;

            string extraFields = askString(PROMPT_EXTRA_FIELDS, settings.extraFields);
            settings.extraFields = extraFields;

            try
            {
                LoginCreateResult queryResult = adminWS.loginCreate(0, serviceId, loginName, firstname, lastname, mail, phone, status, role, access, 2, lang, extraFields);
                if (!queryResult.err.Equals(RESULT_OK))
                {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult.err);
                    return RET_ERROR;
                }

                String longCode = queryResult.code;

                Console.WriteLine("\n3 week activation long code is: {0}", longCode);

                Console.WriteLine("\nGet activation code of the login from long code (function loginGetCodeFromLink)");
                String activationCode = adminWS.loginGetCodeFromLink(longCode);
                Console.WriteLine("Final activation code is: {0}", activationCode);

                Console.WriteLine("\nGet activation code and ID of login from long code (function loginGetInfoFromLink)");
                LoginCreateResult loginInfoResult = adminWS.loginGetInfoFromLink(longCode);
                Console.WriteLine("Final activation code is: {0}", loginInfoResult.code);
                Console.WriteLine("Login ID is: {0}", loginInfoResult.id);

            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }
  
        private int loginUpdate()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nUpdate Login");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;
            
            string firstname = askString(PROMPT_FIRSTNAME, settings.firstname);
            settings.firstname = firstname;

            string lastname = askString(PROMPT_LASTNAME, settings.lastname);
            settings.lastname = lastname;
            
            string mail = askString(PROMPT_MAIL, settings.mail);
            settings.mail = mail;
            
            string phone = askString(PROMPT_PHONE, settings.phone);
            settings.phone = phone;
            
            int status = askInt(PROMPT_STATUS, settings.status);
            settings.status = status;
            
            int role = askInt(PROMPT_ROLE, settings.role);
            settings.role = role;

            string extraFields = askString(PROMPT_EXTRA_FIELDS, settings.extraFields);
            settings.extraFields = extraFields;

            try {
                string queryResult = adminWS.loginUpdate(0, serviceId, loginId, loginName, firstname, lastname, mail, phone, status, role, extraFields);
                if (!queryResult.Equals(RESULT_OK)) { 
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);

            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginSendByMail()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nSend login by mail");
            
            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try {
                string queryResult = adminWS.loginSendByMail(0, serviceId, loginId);
                if (!queryResult.Equals(RESULT_OK)) { 
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginAddDevice()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nGet Secure Site ID for new Device");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try
            {
                string queryResult = adminWS.loginAddDevice(0, serviceId, loginId,0);
                Console.WriteLine("\n{0}", queryResult);
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginResetPwdExtended()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nGet Secure Site ID for Password Reset");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try
            {
                string queryResult = adminWS.loginResetPwdExtended(0, serviceId, loginId, 0);
                Console.WriteLine("\n{0}", queryResult);
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }


        private int loginResetPINErrorCounter()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nReset Password Error Counter");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try
            {
                string queryResult = adminWS.loginResetPINErrorCounter(0, serviceId, loginId);
                Console.WriteLine("\n{0}", queryResult);
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginRestore()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nRestore User");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try
            {
                string queryResult = adminWS.loginRestore(0, serviceId, loginId);
                Console.WriteLine("\n{0}", queryResult);
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginActivateCode()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nGet activation code for a login");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try {
                string queryResult = adminWS.loginActivateCode(0, serviceId, loginId);
                if (queryResult.StartsWith(RESULT_NOK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int loginDelete()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nDelete login");

            int loginId = askInt(PROMPT_LOGIN_ID, settings.loginId);
            settings.loginId = loginId;

            try {
                string queryResult = adminWS.loginDelete(0, serviceId, loginId);
                if (!queryResult.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int authenticate()
        {
            Properties.Settings settings = Properties.Settings.Default;
            Console.WriteLine("\nAuthenticate");

            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string token = askString(PROMPT_TOKEN, settings.token);
            settings.token = token;

            try {
                string queryResult = authWS.authenticate(loginName, serviceId.ToString(), token);
                if (!queryResult.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int authenticateWithIp()
        {
            Properties.Settings settings = Properties.Settings.Default;

            Console.WriteLine("\nAuthenticate with IP");

            string loginName = askString(PROMPT_LOGIN_NAME, settings.loginName);
            settings.loginName = loginName;

            string token = askString(PROMPT_TOKEN, settings.token);
            settings.token = token;

            string ip = askString(PROMPT_IP, settings.ip);
            settings.ip = ip;

            try {
                string queryResult = authWS.authenticateWithIp(loginName, serviceId.ToString(), token, ip);
                if (!queryResult.Equals(RESULT_OK)) {
                    Console.WriteLine(PROMPT_ERROR_FORMAT, queryResult);
                    return RET_ERROR;
                }
                Console.WriteLine("\n{0}", queryResult);
            } catch (System.Net.WebException e) {
                Console.WriteLine(PROMPT_ERROR_FORMAT, e.Message);
                return RET_ERROR;
            }
            return RET_OK;
        }

        private int showMenuProvisioning()
        {
            showMenu(menuProvisioning);
            return RET_OK;
        }

        private int showMenuProvExt()
        {
            showMenu(menuProvExt);
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
            while (prompt(menu, top) != RET_QUIT) ;
        }

        public void start()
        {
            Console.WriteLine("InWebo Api Demo C# prompt");
            showMenu(menuTop, true);
        }
    }
}
