using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text.Json;
using System.IO;
using System.Net.Sockets;

namespace ADUserLister
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage : ADUserLister.exe <IP_AD> <PORT>");
                return;
            }

            string ip = args[0];
            if (!int.TryParse(args[1], out int port))
            {
                Console.WriteLine("[!] Erreur : Port invalide.");
                return;
            }

            // 🔍 Test rapide du port TCP
            if (!TestPort(ip, port))
            {
                Console.WriteLine($"[!] Impossible de se connecter à {ip}:{port} (port fermé ou non routable)");
                return;
            }

            try
            {
                var identifier = new LdapDirectoryIdentifier(ip, port);
                var connection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Negotiate
                };

                connection.SessionOptions.ProtocolVersion = 3;

                Console.WriteLine("[*] Connexion au serveur LDAP...");
                try
                {
                    connection.Bind(); // Peut lancer une LdapException
                }
                catch (LdapException ldapEx)
                {
                    Console.WriteLine($"[!] Erreur de connexion LDAP : {ldapEx.Message}");
                    return;
                }

                // Auto-découverte du DN via RootDSE
                var rootDseRequest = new SearchRequest(
                    "",
                    "(objectClass=*)",
                    SearchScope.Base,
                    "defaultNamingContext"
                );

                var rootDseResponse = (SearchResponse)connection.SendRequest(rootDseRequest);
                string defaultNamingContext = rootDseResponse.Entries[0].Attributes["defaultNamingContext"][0].ToString();

                Console.WriteLine($"[*] Domaine détecté : {defaultNamingContext}");
                Console.Write("Voulez-vous utiliser ce domaine ? (O/n) : ");
                string input = Console.ReadLine()?.Trim().ToLower();

                string baseDN = (input == "n" || input == "non")
                    ? DemanderDNManuellement()
                    : defaultNamingContext;

                Console.WriteLine($"[*] Utilisation de la base DN : {baseDN}");

                // Requête LDAP des utilisateurs
                var searchRequest = new SearchRequest(
                    baseDN,
                    "(objectClass=user)",
                    SearchScope.Subtree,
                    new[] { "sAMAccountName", "userAccountControl", "servicePrincipalName" }
                );

                var response = (SearchResponse)connection.SendRequest(searchRequest);
                var userList = new List<object>();

                foreach (SearchResultEntry entry in response.Entries)
                {
                    string sam = entry.Attributes["sAMAccountName"]?[0].ToString() ?? "N/A";

                    int uac = 0;
                    if (entry.Attributes["userAccountControl"] != null && entry.Attributes["userAccountControl"].Count > 0)
                        int.TryParse(entry.Attributes["userAccountControl"][0].ToString(), out uac);

                    bool hasSPN = entry.Attributes["servicePrincipalName"] != null;
                    bool isDisabled = (uac & 0x0002) != 0;
                    bool isKerberoastable = hasSPN && !isDisabled;

                    userList.Add(new
                    {
                        Username = sam,
                        HasSPN = hasSPN,
                        UserAccountControl = uac,
                        IsDisabled = isDisabled,
                        IsKerberoastable = isKerberoastable
                    });
                }

                string jsonOutput = JsonSerializer.Serialize(userList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("users.json", jsonOutput);

                Console.WriteLine("[+] Export terminé : users.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Erreur critique : {ex.Message}");
            }
        }

        static string DemanderDNManuellement()
        {
            Console.Write("Veuillez entrer manuellement le base DN (ex: DC=mondomaine,DC=local) : ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        static bool TestPort(string ip, int port, int timeoutMs = 3000)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect(ip, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeoutMs);
                if (!success) return false;
                client.EndConnect(result);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}