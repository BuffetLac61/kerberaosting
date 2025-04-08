# ADUserLister

Outil CLI en C# pour lister les comptes utilisateurs d’un serveur Active Directory via LDAP, et détecter ceux vulnérables à **Kerberoasting**.

---

## 🔧 Fonctionnalités

- Connexion à un serveur LDAP en fournissant l’IP et le port.
- Auto-détection du domaine LDAP (`defaultNamingContext`) via RootDSE.
- Demande à l’utilisateur de valider ou saisir manuellement le DN.
- Liste les utilisateurs avec :
  - Présence de SPN
  - Statut activé/désactivé
  - Indication s’ils sont vulnérables au **Kerberoasting**
- Export des résultats au format **JSON**

---

## 🧰 Prérequis

- .NET SDK 6 installé (`dotnet --version` doit retourner `6.x.x`)
- Accès réseau au serveur Active Directory (port LDAP 389 ou LDAPS 636)
- Droits d’accès suffisants pour interroger le serveur LDAP (compte utilisateur domaine standard généralement suffisant)

---

## ⚙️ Compilation

```bash
git clone https://github.com/<votre-repo>/ADUserLister.git
cd ADUserLister
dotnet build