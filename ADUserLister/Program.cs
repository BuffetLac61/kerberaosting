using LdapForNet;
using System.Text.Json;
public class UserInfo
{
    public string Username { get; set; }
    public bool HasSpn { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsKerberoastable { get; set; }
}

class Program
{
    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage : ADUserLister <IP> <PORT>");
            return;
        }

        string ip = args[0];
        if (!int.TryParse(args[1], out int port))
        {
            Console.WriteLine("[!] Port invalide.");
            return;
        }

        try
        {
            // Connexion LDAP
            using var ldap = new LdapConnection();
            ldap.Connect(ip, port);

            Console.WriteLine("[+] Connexion LDAP réussie !");

            // Bind anonyme
            ldap.Bind("simple", null, null);
            Console.WriteLine("[+] Connexion anonyme réussie !");

            // Lecture du RootDSE
            var rootDse = ldap.GetRootDse();

            if (rootDse == null)
            {
                Console.WriteLine("[!] Impossible de récupérer le RootDSE.");
                return;
            }
            Console.WriteLine("[+] RootDSE récupéré avec succès !");
            Console.WriteLine("------------------------------------------RootDSE---------------------------------------------\n");
            string? defaultNamingContext = null;
            foreach (var attr in rootDse.DirectoryAttributes)
            {
                var name = attr.Name;
                var values = attr.GetValues<string>();

                Console.WriteLine($"  {name}: {string.Join(", ", values)}");

                if (name.Equals("defaultNamingContext", StringComparison.OrdinalIgnoreCase))
                    {
                        defaultNamingContext = values.FirstOrDefault();
                    }
            }      
            Console.WriteLine("\n--------------------------------------End of RootDSE----------------------------------------\n");

            // Choix du bon Domain Name
            if (string.IsNullOrEmpty(defaultNamingContext))
            {
                Console.WriteLine("[!] defaultNamingContext non trouvé dans le RootDSE.");
                return;
            }

            Console.WriteLine($"[*] Domaine détecté automatiquement : {defaultNamingContext}");
            Console.Write("Souhaitez-vous l’utiliser ? (O/n) : ");
            var response = Console.ReadLine()?.Trim().ToLower();

            if (response == "n" || response == "non")
            {
                Console.Write("Veuillez entrer le base DN manuellement (ex: DC=corp,DC=local) : ");
                var inputDn = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(inputDn))
                    defaultNamingContext = inputDn;
            }

            var baseDn = defaultNamingContext.ToLowerInvariant();

            Console.WriteLine($"[+] Domaine utilisé pour la recherche : {baseDn}");

            // Authentification avec un utilisateur LDAP pour plus d'informations
            Console.Write("\nEntrez l'identifiant LDAP (ex: administrator@corp.local) (laisser vide pour une analyse anonyme): ");
            var user = Console.ReadLine()?.Trim();

            Console.Write("Mot de passe (laisser vide pour une analyse anonyme): ");
            var password = ReadPassword();
            try
            {
                if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(password))
                {
                    // Bind anonyme
                    ldap.Bind("simple", null, null);
                    Console.WriteLine("[+] Connexion anonyme réussie !");
                }
                else
                {
                    // Bind simple
                    ldap.Bind("simple", user, password);
                    Console.WriteLine("[+] Authentification réussie !");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Échec de l'authentification : {ex.Message}");
                return;
            }

            // Recherche des utilisateurs
	        var entries = ldap.Search(baseDn,"(objectClass=user)");
            Console.WriteLine($"[+] Utilisateurs trouvés : {entries.Count}");

            // Analyse de utilisateurs
            Console.WriteLine("\n[+] Analyse des comptes Kerberoastables :");
            var userResults = new List<UserInfo>();
            foreach (var entry in entries)
            {
                var attrs = entry.DirectoryAttributes;

                string username = attrs.Contains(LdapAttributes.SAmAccountName)
                    ? attrs[LdapAttributes.SAmAccountName].GetValues<string>().FirstOrDefault()
                    : "(inconnu)";

                string uacStr = attrs.Contains(LdapAttributes.UserAccountControl)
                    ? attrs[LdapAttributes.UserAccountControl].GetValues<string>().FirstOrDefault()
                    : "0";

                int.TryParse(uacStr, out int uac);
                bool isDisabled = (uac & 0x0002) != 0;

                bool hasSpn = attrs.Contains("servicePrincipalName");
                bool isKerberoastable = hasSpn && !isDisabled;

                userResults.Add(new UserInfo
                {
                    Username = username,
                    HasSpn = hasSpn,
                    IsDisabled = isDisabled,
                    IsKerberoastable = isKerberoastable
                });


                Console.WriteLine($"Utilisateur : {username}");
                Console.WriteLine($"  - SPN : {(hasSpn ? "✔️ présent" : "❌ absent")}");
                Console.WriteLine($"  - Compte actif : {(isDisabled ? "❌ NON" : "✔️ OUI")}");
                Console.WriteLine($"  - ⚠️ Kerberoastable : {(isKerberoastable ? "🧨 OUI" : "✅ NON")}\n");
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true // format lisible pour l'export JSON
            };

            File.WriteAllText("users.json", JsonSerializer.Serialize(userResults, jsonOptions));
            Console.WriteLine("[+] Export JSON terminé : users.json");

            ldap.Dispose(); // Plus propre
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Erreur de connexion : {ex.Message}");
        }
    }
}