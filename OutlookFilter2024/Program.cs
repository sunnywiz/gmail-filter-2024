using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookFilter2024
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // use outlook primary interop to get all mail messages from sunnywiz@gmail.com account in outlook
            var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
        
            // get list of accounts that outlookApp is configured to work with
            var accounts = outlookApp.Session.Accounts;
        
            var outlookNamespace = outlookApp.GetNamespace("MAPI");
            
            // get list of folders in outlookNamespace
            var folders = outlookNamespace.Folders;
            // get the folder for sunnywiz@gmail.com from folders
            var emailAccount = folders["sunnywiz@gmail.com"];
            // iterate through all the folders and subfolders of emailAccount
            var introspectList = emailAccount.Folders.Cast<Microsoft.Office.Interop.Outlook.Folder>().ToList();
            while (introspectList.Count > 0)
            {
                var folder = introspectList[0];
                introspectList.RemoveAt(0);
                introspectList.AddRange(folder.Folders.Cast<Microsoft.Office.Interop.Outlook.Folder>());
                Console.WriteLine($"Folder: {folder.FullFolderPath}  with {folder.Items.Count} items");
                // introspect items within folder
            }
            
            
            Console.WriteLine("Stop here");
        }
    }
}
