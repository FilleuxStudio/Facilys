const mariadb = require('mariadb');
const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');
const crypto = require('crypto');

class DatabaseService {
  constructor() {
    this.apiKey = process.env.PLANETHOSTER_API_KEY;
    this.apiUser = process.env.PLANETHOSTER_API_USER;
    this.baseUrl = 'https://api.planethoster.net/v3'; // Assurez-vous d'utiliser la bonne version de l'API
  }

  async createDatabase(userId) {
    const dbName = `user_${userId}_db`;
    const dbUser = `user_${userId}`;
    const dbPassword = this.generateSecurePassword();

    const endpoint = `${this.baseUrl}/databases`;

    const data = {
      name: dbName,
      username: dbUser,
      password: dbPassword,
      type: 'mariadb' // ou 'mysql' selon votre besoin
    };

    const headers = {
      'X-API-KEY': this.apiKey,
      'X-API-USER': this.apiUser,
      'Content-Type': 'application/json'
    };

    try {
      const response = await axios.post(endpoint, data, { headers: headers });

      if (response.status === 200 || response.status === 201) {
        console.log('Base de données créée avec succès via PlanetHoster API');

        // Exécuter le script SQL sur la base nouvellement créée
        await this.executeSQLScript({
          host: process.env.DB_HOST, // L'hôte du serveur MariaDB/MySQL
          user: dbUser,
          password: dbPassword,
          database: dbName
        });

        return {
          name: dbName,
          user: dbUser,
          password: dbPassword
        };
      } else {
        console.error('Erreur lors de la création de la base de données via PlanetHoster API:', response.data);
        throw new Error(`Erreur API: ${response.data.message}`);
      }
    } catch (error) {
      console.error('Erreur lors de l\'appel à l\'API PlanetHoster:', error);
      throw error;
    }
  }

  async executeSQLScript(connectionConfig) {
    let conn;

    try {
      // Charger le script SQL
      const scriptPath = path.join(__dirname, '..', 'sql', 'init_db.sql');
      const sqlScript = await fs.readFile(scriptPath, 'utf8');

      // Établir une connexion à la base de données
      conn = await mariadb.createConnection(connectionConfig);

      // Diviser le script en instructions SQL individuelles
      const statements = sqlScript.split(/;\s*(?=CREATE|INSERT|ALTER|DROP|UPDATE|DELETE|SELECT)/i)
                                  .filter(statement => statement.trim() !== '');

      // Exécuter chaque instruction SQL individuellement
      for (const statement of statements) {
        if (statement.trim()) {
          await conn.query(statement);
        }
      }

      console.log('Script SQL exécuté avec succès sur la base de données:', connectionConfig.database);
    } catch (error) {
      console.error('Erreur lors de l\'exécution du script SQL:', error);
      throw error;
    } finally {
      if (conn) await conn.end(); // Fermer la connexion
    }
  }

  generateSecurePassword() {
    return crypto.randomBytes(16).toString('hex');
  }

  async close() {
    console.log('Fermeture du service de base de données (API)');
  }
}

module.exports = new DatabaseService();
