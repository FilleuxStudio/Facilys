const mariadb = require('mariadb');
const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');
const crypto = require('crypto');

class DatabaseService {
  constructor() {
    this.apiKey = process.env.PLANETHOSTER_API_KEY;
    this.apiUser = process.env.PLANETHOSTER_API_USER;
    this.worldAccountId = process.env.WORLDACCOUNTS; 
    this.baseUrl = 'https://api.planethoster.net/v3';
  }

  /**
   * Crée une base de données et un utilisateur associé avec tous les privilèges, puis exécute un script SQL d'initialisation.
   * @param {string|number} userId - L'identifiant de l'utilisateur dans votre application.
   * @returns {Promise<Object>} - Objet contenant le nom de la DB, le nom d'utilisateur et le mot de passe généré.
   */
  async createDatabase(userId) {
    // Génération des identifiants et mot de passe
    const dbName = `user_${userId}_db`;
    const dbUser = `user_${userId}`;
    const dbPassword = this.generateSecurePassword();

    // Configuration des headers pour les appels API
    const headers = {
      'X-API-KEY': this.apiKey,
      'X-API-USER': this.apiUser,
      'Content-Type': 'application/json'
    };

    try {
      // 1. Création de la base de données
      const createDbUrl = `${this.baseUrl}/hosting/database`;
      const createDbPayload = {
        id: this.worldAccountId,
        name: dbName,
        databaseType: 'MYSQL'
      };

      const dbResponse = await axios.post(createDbUrl, createDbPayload, { headers });
      if (!(dbResponse.status === 200 || dbResponse.status === 201)) {
        throw new Error(`Erreur lors de la création de la DB : ${dbResponse.data.message}`);
      }
      console.log(`Base de données "${dbName}" créée avec succès via l'API PlanetHoster.`);

      // 2. Création de l'utilisateur associé
      const createUserUrl = `${this.baseUrl}/hosting/database/user`;
      const createUserPayload = {
        id: this.worldAccountId,
        dbUser: dbUser,
        password: dbPassword,
        databaseType: 'MYSQL'
      };

      const userResponse = await axios.post(createUserUrl, createUserPayload, { headers });
      if (!(userResponse.status === 200 || userResponse.status === 201)) {
        throw new Error(`Erreur lors de la création de l'utilisateur : ${userResponse.data.message}`);
      }
      console.log(`Utilisateur "${dbUser}" créé avec succès via l'API PlanetHoster.`);

      // 3. Attribution des privilèges à l'utilisateur sur la base
      const grantPrivilegesUrl = `${this.baseUrl}/hosting/database/user/privileges`;
      const grantPayload = {
        databaseType: 'MYSQL',
        privileges: 'ALL PRIVILEGES',
        id: this.worldAccountId,
        databaseName: dbName,
        databaseUsername: dbUser
      };

      const grantResponse = await axios.put(grantPrivilegesUrl, grantPayload, { headers });
      if (!(grantResponse.status === 200 || grantResponse.status === 201)) {
        throw new Error(`Erreur lors de l'attribution des privilèges : ${grantResponse.data.message}`);
      }
      console.log(`Privilèges "ALL PRIVILEGES" accordés à "${dbUser}" sur "${dbName}".`);

      // 4. Exécution du script SQL d'initialisation
      await this.executeSQLScript({
        host: "127.0.0.1", // Hôte du serveur MariaDB/MySQL
        user: dbUser,
        password: dbPassword,
        database: dbName
      });

      return {
        name: dbName,
        user: dbUser,
        password: dbPassword
      };

    } catch (error) {
      console.error('Erreur dans createDatabase :', error);
      throw error;
    }
  }

  /**
   * Exécute un script SQL (contenu dans un fichier) pour initialiser la base de données.
   * @param {Object} connectionConfig - La configuration de connexion {host, user, password, database}.
   */
  async executeSQLScript(connectionConfig) {
    let conn;
    try {
      // Charger le script SQL depuis le fichier (assurez-vous que le chemin est correct)
      const scriptPath = path.join(__dirname, '..', 'sql', 'init_db.sql');
      const sqlScript = await fs.readFile(scriptPath, 'utf8');

      // Établir une connexion à la base de données
      conn = await mariadb.createConnection(connectionConfig);

      // Découper le script en instructions individuelles
      const statements = sqlScript
        .split(/;\s*(?=CREATE|INSERT|ALTER|DROP|UPDATE|DELETE|SELECT)/i)
        .map(statement => statement.trim())
        .filter(statement => statement.length > 0);

      // Exécuter chaque instruction SQL
      for (const statement of statements) {
        await conn.query(statement);
      }
      console.log(`Script SQL exécuté avec succès sur la base "${connectionConfig.database}".`);
    } catch (error) {
      console.error('Erreur lors de l\'exécution du script SQL :', error);
      throw error;
    } finally {
      if (conn) await conn.end();
    }
  }

  /**
   * Génère un mot de passe sécurisé.
   * @returns {string} - Mot de passe généré.
   */
  generateSecurePassword() {
    return crypto.randomBytes(16).toString('hex');
  }

  /**
   * Méthode de fermeture du service si besoin.
   */
  async close() {
    console.log('Fermeture du service DatabaseService');
  }
}

module.exports = new DatabaseService();