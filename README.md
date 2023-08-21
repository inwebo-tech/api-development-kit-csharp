# In-Webo Web Services API C Sharp Development Kit

## Description

This package, provided by In-Webo Technologies, includes a set of C# sample code to integrate In-Webo Authentication and Provisioning APIs in any C# application.

## Getting Started

Before you start writing code, you need to have an InWebo admin account. You can get one at: http://www.myinwebo.com/signup 
When logged in to InWebo WebConsole, go to "service parameters". From this screen, you will be able to get:
- your service_id
- a certificate file

These 2 items are mandatory. Once your have them, open Program.cs, and fill in the 3 variables below with the correct values:
private static string p12file = "path_to_your_certificate.p12"; // Specify here the name of your certificate file.
private static string p12password = "your_password"; // This is the password to access your certificate file
private static long serviceId = 0; // This is the id of your service.

## Development environment setup

In order to build this project under Visual Studio 10, you need to update the 
web references. Everywhere in the project, the paths are relative to the root 
of the project, but the Web References must be absolute paths. 

In "Solution explorer", expand the project ApiDemo, then the folder 
"Web References". 
For each Reference (Authentication and Provisioning), right click on it and 
select Properties. 
Then in the Properties window, change the Web Reference URL. 
Replace "D:\Dev\iwapi\wsdl" by the absolute path of the directory containing wsdl files.
 
You can now update the Web references, if necessary (i.e, if you 
change the wsdl files in the directory wsdl).


Next, run the project to make sure that your settings are ok.
