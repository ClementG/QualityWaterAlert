# Déploiement sur NAS TerraMaster

Guide pas-à-pas pour déployer QualityWaterAlert sur un NAS TerraMaster via Docker.

---

## Prérequis

- NAS TerraMaster avec **TOS 5.1 ou supérieur**
- Accès administrateur à l'interface TOS
- Le NAS est connecté à votre réseau local (IP locale fixe recommandée)
- Un ordinateur sur le même réseau pour les opérations de configuration

---

## Partie 1 — Préparation du NAS

### 1.1 Attribuer une IP locale fixe au NAS

Sur votre routeur (box internet), réservez une adresse IP fixe pour le NAS via son adresse MAC.  
Notez cette IP (ex. `192.168.1.50`) — elle sera utilisée dans toute la suite.

### 1.2 Activer SSH

1. Ouvrez l'interface TOS → **Panneau de configuration** → **Terminal**
2. Cochez **Activer le service SSH**
3. Laissez le port par défaut (`22`) ou changez-le (ex. `2222`) pour réduire le bruit
4. Cliquez **Appliquer**

Testez la connexion depuis votre PC :
```bash
ssh admin@192.168.1.50
```

### 1.3 Installer Docker

1. TOS → **App Center**
2. Recherchez **Docker** et cliquez **Installer**
3. Une fois installé, ouvrez Docker depuis le menu principal — vérifiez que le démon est actif

---

## Partie 2 — Déploiement de l'application

### 2.1 Créer le dossier de l'application sur le NAS

Connectez-vous en SSH :
```bash
ssh admin@192.168.1.50
```

Créez un dossier dédié dans un volume partagé :
```bash
mkdir -p /vol1/docker/qualitywateralert
cd /vol1/docker/qualitywateralert
```

> **Note** : `/vol1` est le volume principal par défaut sur TerraMaster. Adaptez si nécessaire (`/vol2`, etc.).

### 2.2 Copier les fichiers du projet

**Option A — depuis votre PC via SCP :**
```bash
# Depuis votre PC (dans le dossier du projet)
scp -r docker-compose.yml src/ admin@192.168.1.50:/vol1/docker/qualitywateralert/
```

**Option B — clone Git (si Git est disponible sur le NAS) :**
```bash
# Sur le NAS, en SSH
cd /vol1/docker/qualitywateralert
git clone https://github.com/ClementG/QualityWaterAlert.git .
```

**Option C — via le Gestionnaire de fichiers TOS :**  
Uploadez manuellement `docker-compose.yml` et le dossier `src/` via l'interface web TOS.

### 2.3 Créer le fichier de configuration `.env`

Sur le NAS, dans le dossier de l'application :
```bash
cd /vol1/docker/qualitywateralert
cp .env.example .env   # si vous avez copié le fichier exemple
# ou créez-le manuellement :
nano .env
```

Remplissez les variables (voir `.env.example` pour la liste complète).  
Les variables SMTP sont optionnelles si vous n'utilisez pas encore les alertes email.

### 2.4 Lancer l'application

```bash
cd /vol1/docker/qualitywateralert
docker compose up -d --build
```

> Le premier build télécharge les images .NET (~300 Mo) et compile le projet (~2-3 min).

Vérifiez que le conteneur tourne :
```bash
docker compose ps
# STATUS doit afficher "healthy" après ~30 secondes
```

Consultez les logs si nécessaire :
```bash
docker compose logs -f webapp
```

### 2.5 Tester en local

Ouvrez un navigateur sur votre réseau local :
```
http://192.168.1.50:8080
```

L'application doit s'afficher. Si ce n'est pas le cas, vérifiez les logs avec la commande ci-dessus.

---

## Partie 3 — Accès depuis l'extérieur

Deux approches sont possibles. **Cloudflare Tunnel est recommandé** : plus simple et aucun port ouvert sur votre box.

---

### Option A — Cloudflare Tunnel (recommandé)

Avantages : aucun port à ouvrir, HTTPS automatique, protection DDoS Cloudflare incluse, gratuit.

#### A.1 Prérequis

- Un nom de domaine géré par **Cloudflare** (gratuit avec un domaine existant ou ~10 €/an pour un `.fr`)
- Un compte Cloudflare gratuit sur [dash.cloudflare.com](https://dash.cloudflare.com)

#### A.2 Créer le tunnel

1. Dans Cloudflare Dashboard → **Zero Trust** → **Networks** → **Tunnels**
2. Cliquez **Create a tunnel** → donnez-lui un nom (ex. `nas-home`)
3. Cloudflare génère un token — copiez-le

#### A.3 Déployer `cloudflared` sur le NAS

Ajoutez ce service dans votre `docker-compose.yml` :

```yaml
  cloudflared:
    image: cloudflare/cloudflared:latest
    restart: unless-stopped
    command: tunnel --no-autoupdate run
    environment:
      TUNNEL_TOKEN: "${CLOUDFLARE_TUNNEL_TOKEN}"
```

Ajoutez `CLOUDFLARE_TUNNEL_TOKEN=votre_token` dans votre `.env`.

```bash
docker compose up -d cloudflared
```

#### A.4 Configurer la route publique

Dans Cloudflare Dashboard → votre tunnel → **Public Hostname** :

| Champ       | Valeur                          |
|-------------|---------------------------------|
| Subdomain   | `water` (ou ce que vous voulez) |
| Domain      | `votredomaine.fr`               |
| Type        | `HTTP`                          |
| URL         | `webapp:8080`                   |

> Cloudflare fait le HTTPS → HTTP en interne. Aucun certificat à gérer.

Votre app sera accessible à `https://water.votredomaine.fr`.

---

### Option B — Port Forwarding + DDNS

Avantages : pas besoin de domaine Cloudflare. Inconvénients : ouvre des ports sur votre box, nécessite une gestion DDNS si votre IP change.

#### B.1 Configurer le DDNS

**Avec DuckDNS (gratuit) :**

1. Créez un compte sur [duckdns.org](https://www.duckdns.org) et réservez un sous-domaine (ex. `monapp.duckdns.org`)
2. Sur le NAS, créez un conteneur de mise à jour DDNS :

```yaml
  ddns-updater:
    image: qmcgaw/ddns-updater:latest
    restart: unless-stopped
    volumes:
      - /vol1/docker/ddns-updater:/updater/data
```

3. Configurez DuckDNS dans `/vol1/docker/ddns-updater/config.json` (voir [doc ddns-updater](https://github.com/qdm12/ddns-updater)).

#### B.2 Configurer un reverse proxy HTTPS sur le NAS

Installez **nginx Proxy Manager** pour gérer automatiquement les certificats Let's Encrypt :

```yaml
  nginx-proxy-manager:
    image: jc21/nginx-proxy-manager:latest
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
      - "81:81"   # interface d'administration
    volumes:
      - /vol1/docker/npm/data:/data
      - /vol1/docker/npm/letsencrypt:/etc/letsencrypt
```

Dans l'interface nginx Proxy Manager (`http://192.168.1.50:81`) :
1. Ajoutez un **Proxy Host** → domaine `monapp.duckdns.org`, Forward Host `webapp`, Port `8080`
2. Onglet **SSL** → cochez **Request a new SSL Certificate** (Let's Encrypt)

#### B.3 Ouvrir les ports sur votre box internet

Dans l'interface de votre box/routeur, ajoutez des règles de **NAT/redirection de port** :

| Port externe | Port interne | IP destination  | Protocole |
|--------------|--------------|-----------------|-----------|
| 80           | 80           | 192.168.1.50    | TCP       |
| 443          | 443          | 192.168.1.50    | TCP       |

Votre app sera accessible à `https://monapp.duckdns.org`.

---

## Partie 4 — Mise à jour de l'application

```bash
cd /vol1/docker/qualitywateralert

# Récupérer les dernières modifications (si clone Git)
git pull origin master

# Reconstruire et relancer
docker compose up -d --build
```

Le conteneur redémarre avec la nouvelle version sans interruption de service prolongée.

---

## Commandes utiles

```bash
# Statut des conteneurs
docker compose ps

# Logs en temps réel
docker compose logs -f webapp

# Arrêter l'application
docker compose down

# Arrêter et supprimer les images
docker compose down --rmi all

# Redémarrer sans rebuild
docker compose restart webapp

# Inspecter l'utilisation des ressources
docker stats
```
