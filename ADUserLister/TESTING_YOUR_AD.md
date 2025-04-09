# 🧪 Guide de test – Installer Active Directory sur une VM avec VMware

Ce guide permet à un évaluateur de **reproduire un environnement de test Active Directory (AD)** pour valider le bon fonctionnement de l’outil `ADUserLister`, depuis une machine Linux ou Windows hôte.

---

## 🖥️ 1. Prérequis matériel et logiciel

- VMware Workstation ou VMware Player (Linux ou Windows)
- Une **ISO Windows Server 2019/2022** (évaluation gratuite sur Microsoft)
- Une licence d’évaluation ou clé générique : `N69G4-B89J2-4G8F4-WWYCC-J464C`
- Connexion Internet pour les mises à jour Windows

---

## 🧱 2. Création de la VM AD

### a. Créer une nouvelle VM dans VMware
- Choisir **Custom (avancé)**
- Choisir **Installer à partir d’une image ISO**
- Donner 2 CPUs, 4 Go de RAM minimum
- 60 Go de disque suffisent
- Installer **Windows Server** (Standard ou Datacenter)

### b. Choisir le mode réseau
- Recommandé : **NAT** pour un accès Internet et accès local depuis l’hôte
  - Alternative : **Host-only** (plus sécurisé, sans accès Internet)

---

## 🏗️ 3. Configuration du serveur

### a. Nommer le serveur
- Ouvrir PowerShell ou le Panneau Système
- Ex : `WIN-AD01` ou `AD-TEST-SRV`

### b. Définir une IP statique (en NAT ou Host-only)
- Ex : IP `192.168.146.138`, masque `255.255.255.0`, passerelle `192.168.146.2`
- DNS : l’adresse IP du serveur lui-même (ex : `192.168.146.138`)

### c. Activer les fonctionnalités AD DS
Dans PowerShell Admin :

```powershell
Install-WindowsFeature AD-Domain-Services -IncludeManagementTools
```

Puis, promouvoir en contrôleur de domaine :

```powershell
Import-Module ADDSDeployment
Install-ADDSForest -DomainName "corp.local"
```

**Redémarrer la VM** à la fin du processus.

### d. Autoriser les connexions LDAP dans le pare-feu

Toujours dans PowerShell admin :

```powershell
New-NetFirewallRule -DisplayName "Allow LDAP" -Direction Inbound -Protocol TCP -LocalPort 389 -Action Allow
```

---

## 👤 4. Création des utilisateurs de test

### a. Connexion en tant que `Administrator` (mot de passe défini à l’installation)

### b. Créer des utilisateurs via PowerShell
```powershell
New-ADUser -Name "user1" -AccountPassword (ConvertTo-SecureString "P@ssw0rd!" -AsPlainText -Force) -Enabled $true
New-ADUser -Name "svc_sql" -AccountPassword (ConvertTo-SecureString "P@ssw0rd!" -AsPlainText -Force) -Enabled $true -ServicePrincipalNames @{Add="MSSQLSvc/srv1.corp.local:1433"}
```

### c. Vérifier les SPNs et comptes actifs
```powershell
Get-ADUser -Filter * -Properties Enabled,ServicePrincipalName | Format-Table Name,Disabled,SPN
```

Ou version simplifiée :
```powershell
Get-ADUser -Filter * -Properties ServicePrincipalName,Enabled | Format-Table Name,Enabled,ServicePrincipalName
```

---

## 🌐 5. Vérification du réseau avec la machine hôte

Sur la machine hôte (Linux ou Windows) où est exécuté `ADUserLister` :

```bash
ping 192.168.146.138
```

Puis tester la connectivité LDAP avec :
```bash
telnet 192.168.146.138 389
```
Ou avec l’outil directement :
```bash
dotnet run -- 192.168.146.138 389
```

---

## 📚 Résumé des utilisateurs recommandés pour le test

| Utilisateur | SPN présent | Activé | Mot de passe |
|------------|-------------|--------|--------------|
| Administrator | ❌         | ✅     | Celui défini à l’install |
| user1       | ❌         | ✅     | `P@ssw0rd!`    |
| svc_sql     | ✅         | ✅     | `P@ssw0rd!`    |
| krbtgt      | ✅ (par défaut) | ❌  | Système        |
| Guest       | ❌         | ❌     | -              |

---

## 🧼 Nettoyage (facultatif)

- Créer un snapshot après configuration AD
- Exporter la VM pour réutilisation future

---

**Ce guide permet de valider la détection de comptes Kerberoastables dans un environnement contrôlé.**

