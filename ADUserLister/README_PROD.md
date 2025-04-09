# 📘 ADUserLister – Mode d'emploi pour utilisation en production

Cet outil permet de **lister les utilisateurs d’un serveur Active Directory (AD)** et de détecter ceux qui sont **vulnérables aux attaques Kerberoasting**.

Il est conçu pour une utilisation **simple, rapide, sécurisée** par les administrateurs ou auditeurs.

---

## ✅ Ce que fait l'outil

- Se connecte à un serveur LDAP (AD) via IP + port
- S’authentifie avec vos identifiants (ou anonymement si laissé vide)
- Récupère les utilisateurs AD
- Détecte les comptes Kerberoastables
- Génère un fichier **`users.json`** avec les résultats

---

## ⚙️ Comment l'utiliser

### 1. Prérequis
- Machine avec **.NET 6+** installé
- Accès réseau au serveur AD

### 2. Lancer l’outil

Depuis le dossier du projet, exécutez :

```bash
dotnet run -- <IP_DU_SERVEUR_AD> <PORT>
```

Exemple :
```bash
dotnet run -- 192.168.1.100 389
```

### 3. Authentification

- Il vous sera demandé un **login** (ex: `admin@entreprise.local`) et un **mot de passe**
- Si vous laissez vide, la tentative sera faite en **mode anonyme** (si autorisé par l’AD)

### 4. Sortie du scan

- Affichage clair à l’écran de chaque utilisateur analysé
- Export d’un fichier **`users.json`** contenant les résultats :

```json
[
  {
    "Username": "svc_sql",
    "HasSpn": true,
    "IsDisabled": false,
    "IsKerberoastable": true
  },
  ...
]
```

---

## 🛡️ Comment lire le rapport

| Champ               | Signification                              |
|--------------------|---------------------------------------------|
| `Username`         | Nom d'utilisateur                          |
| `HasSpn`           | Présence d’un SPN                          |
| `IsDisabled`       | Compte désactivé ou non                    |
| `IsKerberoastable` | ✅ Si le compte est actif + SPN présent    |

Un compte **Kerberoastable** peut être ciblé pour extraire un ticket TGS et brute-forcer son mot de passe ➜ à **corriger rapidement** !

---

## 📦 Où trouver le résultat ?

- Dans le fichier : `users.json` (dans le même dossier que l’exécutable)

---

## 🔒 Sécurité

- Le mot de passe n’est **jamais affiché** ni stocké
- L’outil ne modifie **aucune donnée** sur le serveur LDAP
- Code source 100% visible et auditable

---

**Développé dans le cadre d’un test technique. Usage interne uniquement.**

