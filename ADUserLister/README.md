# 🔐 ADUserLister – Scanner d'utilisateurs Active Directory + détection Kerberoastable

Un outil en **C# cross-platform** pour se connecter à un serveur LDAP (Active Directory), **lister les utilisateurs** et **identifier les comptes vulnérables aux attaques de type Kerberoasting**.

✅ Compatible Linux et Windows  
✅ Connexion LDAP par IP + port  
✅ Support de l’authentification anonyme ou simple (login/mdp)  
✅ Génère un rapport JSON avec les attributs clés

---

## ✨ Fonctionnalités

- Connexion à un serveur LDAP via IP + port
- Authentification simple ou anonyme (fallback automatique)
- Lecture du `RootDSE` pour récupérer dynamiquement le `defaultNamingContext`
- Recherche des objets `user`
- Extraction des attributs :
  - `sAMAccountName`
  - `userAccountControl`
  - `servicePrincipalName`
- Détection des comptes **Kerberoastables**
- Export du résultat au format **`users.json`**

---

## ⚙️ Utilisation

### 🧑‍💻 Lancer le programme

```bash
dotnet run <IP_AD> <PORT>
```

Exemple :

```bash
dotnet run 192.168.1.10 389
```

### 🔐 Authentification

- Le programme vous demandera :
  - Un identifiant LDAP (ex : `administrator@corp.local`)
  - Un mot de passe (non affiché à l'écran)
- Si vous **laissez vide**, un **bind anonyme** sera tenté.

---

## 📂 Exemple de sortie JSON

```json
[
  {
    "Username": "svc_sql",
    "HasSpn": true,
    "IsDisabled": false,
    "IsKerberoastable": true
  },
  {
    "Username": "user1",
    "HasSpn": false,
    "IsDisabled": false,
    "IsKerberoastable": false
  }
]
```

---

## 🧠 Fonctionnement technique

L’outil repose sur la bibliothèque **open-source [LdapForNet](https://github.com/flamencist/ldap4net)**.

La version utilisée ici est **intégrée au projet directement sous forme de fichiers `.cs`** dans le dossier `libs/LdapForNet/`.

### 🔍 Détection Kerberoasting

Un utilisateur est considéré comme **vulnérable** s’il :
- possède un attribut `servicePrincipalName`
- et **n’est pas désactivé** (`userAccountControl & 0x0002 == 0`)

---

## 🛠️ Installation

### 1. Prérequis

- .NET SDK 6.0+
- Git (optionnel pour cloner le dépôt)

### 2. Vérifier ou installer .NET 6

```bash
# Vérifier la version installée
$ dotnet --version

# Installer si besoin (Linux):
# https://learn.microsoft.com/fr-fr/dotnet/core/install/linux
```

### 3. Cloner le projet

```bash
git clone https://github.com/BuffetLac61/kerberaosting.git
cd ADUserLister
```

### 4. Lancer le projet

```bash
dotnet build
dotnet run <IP> <PORT>
```

---

## 📦 Structure du projet

```
ADUserLister/
├── Program.cs
├── ADUserLister.csproj
├── users.json                ← résultat JSON généré
└── LdapForNet/         ← version locale de la bibliothèque
    ├── LdapConnection.cs
    ├── ...
```

---

## 🛡️ Dépendances

Aucune dépendance NuGet externe.  
La bibliothèque LdapForNet a été intégrée manuellement pour une **portabilité maximale** (Linux, Windows, offline).

---

## 📄 Licence

Projet à but pédagogique / démonstratif dans le cadre d’un entretien technique.  
Crédits à [flamencist](https://github.com/flamencist/ldap4net) pour LdapForNet.

