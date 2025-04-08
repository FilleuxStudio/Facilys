const mariadb = require("mariadb");
const axiosLib = require("axios");
const fs = require("fs").promises;
const path = require("path");
const crypto = require("crypto");

class DatabaseService {
  constructor() {
    // Vérification des variables d'environnement
    const { PLANETHOSTER_API_KEY, PLANETHOSTER_API_USER, WORLDACCOUNTS } =
      process.env;
    if (!PLANETHOSTER_API_KEY || !PLANETHOSTER_API_USER || !WORLDACCOUNTS) {
      throw new Error(
        "Les variables d'environnement pour l'API PlanetHoster ne sont pas définies."
      );
    }
    this.apiKey = PLANETHOSTER_API_KEY;
    this.apiUser = PLANETHOSTER_API_USER;
    this.worldAccountId = WORLDACCOUNTS;
    this.baseUrl = "https://api.planethoster.net/v3";
    this.suffix = "jmaqmsnt_";

    // Configuration de l'instance Axios
    this.apiClient = axiosLib.create({
      baseURL: this.baseUrl,
      headers: {
        "X-API-KEY": this.apiKey,
        "X-API-USER": this.apiUser,
        "Content-Type": "application/json",
      },
    });
  }

  /**
   * Attends jusqu'à ce qu'une base de données soit confirmée comme créée.
   */
  async waitForDatabaseCreation(databaseName, retries = 10, delay = 3000) {
    for (let i = 0; i < retries; i++) {
      try {
        const response = await this.apiClient.get("/hosting/databases", {
          params: { id: this.worldAccountId, databaseType: "MYSQL" },
        });

        if (response.data.data.some((db) => db.name === databaseName)) {
          console.log(`✅ Base de données "${databaseName}" confirmée.`);
          return;
        }
      } catch (error) {
        console.error(
          "Erreur lors de la récupération des bases de données :",
          error
        );
      }

      console.log(
        `⏳ En attente de la base "${databaseName}"... (${i + 1}/${retries})`
      );
      await new Promise((res) => setTimeout(res, delay));
    }
    throw new Error(
      `⛔ Timeout : La base "${databaseName}" n'a pas été trouvée après plusieurs tentatives.`
    );
  }

  /**
   * Attends jusqu'à ce qu'un utilisateur soit confirmé comme créé.
   */
  async waitForUserCreation(dbUser, retries = 10, delay = 3000) {
    for (let i = 0; i < retries; i++) {
      try {
        const response = await this.apiClient.get("/hosting/databases/users", {
          params: { id: this.worldAccountId, databaseType: "MYSQL" },
        });

        const usersList = response.data.data.flat(); // Aplatir les sous-tableaux en un seul
        if (usersList.includes(dbUser)) {
          console.log(`✅ Utilisateur "${dbUser}" confirmé.`);
          return;
        }
      } catch (error) {
        console.error(
          "Erreur lors de la récupération des utilisateurs :",
          error
        );
      }

      console.log(
        `⏳ En attente de l'utilisateur "${dbUser}"... (${i + 1}/${retries})`
      );
      await new Promise((res) => setTimeout(res, delay));
    }
    throw new Error(
      `⛔ Timeout : L'utilisateur "${dbUser}" n'a pas été trouvé après plusieurs tentatives.`
    );
  }

  /**
 * Attends jusqu'à ce que les privilèges spécifiés soient confirmés pour un utilisateur sur une base de données.
 */
  async waitForUserAssignment(databaseName, dbUser, retries = 10, delay = 3000) {
    var prefixedDbName = `${this.suffix}${databaseName}`;
    var prefixedDbUser = `${this.suffix}${dbUser}`;
  
    for (let i = 0; i < retries; i++) {
      try {
        const response = await this.apiClient.get("/hosting/databases", {
          params: {
            id: this.worldAccountId,
            databaseType: "MYSQL",
          },
        });
  
        const database = response.data.data.find((db) => db.name === prefixedDbName);
        if (database) {
          const user = database.databaseUsers.find((user) => user.name === prefixedDbUser);
          if (user) {
            console.log(`✅ L'utilisateur "${dbUser}" est bien assigné à la base "${databaseName}".`);
            return;
          }
        }
      } catch (error) {
        console.error("❌ Erreur lors de la récupération des bases de données :", error);
      }
  
      console.log(`⏳ En attente de l'assignation de l'utilisateur "${dbUser}" à la base "${databaseName}"... (${i + 1}/${retries})`);
      await new Promise((res) => setTimeout(res, delay));
    }
  
    throw new Error(`⛔ Timeout : L'utilisateur "${dbUser}" n'a pas été assigné à la base "${databaseName}" après plusieurs tentatives.`);
  }
  

  /**
   * Crée une base de données, un utilisateur et leur attribue les privilèges nécessaires.
   */
  async createDatabase(userId) {
    const dbName = `user_${userId}_db`;
    const dbUser = `user_${userId}`;
    var dbPassword = this.generateSecurePassword();

    const prefixedDbName = `${this.suffix}${dbName}`;
    const prefixedDbUser = `${this.suffix}${dbUser}`;

    try {
    // 2️⃣ Création de l'utilisateur
      console.log(`🚀 Création de l'utilisateur "${dbUser}"...`);
      await this.apiClient.post("/hosting/database/user", {
        id: this.worldAccountId,
        dbUser: dbUser,
        password: dbPassword,
        databaseType: "MYSQL",
      });

      await this.waitForUserCreation(prefixedDbUser);

      // 1️⃣ Création de la base de données
      console.log(`🚀 Création de la base de données "${dbName}"...`);
      await this.apiClient.post("/hosting/database", {
        id: this.worldAccountId,
        name: dbName,
        databaseType: "MYSQL",
      });

      await this.waitForDatabaseCreation(prefixedDbName);

      // 3️⃣ Attribution des privilèges à l'utilisateur sur la base
      console.log(
        `🚀 Attribution des privilèges à "${dbUser}" sur "${dbName}"...`
      );
      await this.apiClient.put("/hosting/database/user/privileges", {
        databaseType: "MYSQL",
        privileges: ["ALL PRIVILEGES"],
        id: this.worldAccountId,
        databaseName: prefixedDbName,
        databaseUsername: prefixedDbUser,
      });

      console.log(`✅ Privilèges accordés avec succès.`);

      //await this.waitForUserAssignment(dbName, dbUser);

      // 4️⃣ Exécution du script SQL d'initialisation

      const connectionConfig = {
        host: "localhost",
        user: prefixedDbUser,
        password: dbPassword,
        database: prefixedDbName,
        multipleStatements: true,
        connectTimeout: 5000
      };

     await this.executeSQLScript(connectionConfig) 

      return {
        dbUser: prefixedDbUser,
        dbName: prefixedDbName,
        password: dbPassword,
      };
    } catch (error) {
      console.error("❌ Erreur dans createDatabase :", error);
      throw error;
    }
  }

  /**
   * Exécute un script SQL pour initialiser la base de données.
   */
  async executeSQLScript(connectionConfig) {
    let conn;
    try {
      console.log(`🔍 Test de connexion`);
      conn = await mariadb.createConnection(connectionConfig);
      
      // Vérification de la version du serveur
      const serverVersion = await conn.query('SELECT VERSION() AS version');
      console.log(`📌 Version MariaDB: ${serverVersion[0].version}`);

      // Validation du fichier SQL
      const scriptPath = path.join(__dirname, '../sql/', 'init_db.sql');
      await this.validateScriptFile(scriptPath);

      // Lecture et exécution du script
      const sqlScript = await fs.readFile(scriptPath, 'utf8');
      console.log('🚀 Début de l\'exécution du script SQL...');
      
      const res = await conn.query(sqlScript);
      console.log(`ℹ️ ${res.affectedRows} opérations effectuées`);
      
    } catch (error) {
      console.error('ERREUR DÉTAILLÉE :', {
        code: error.code,
        sqlState: error.sqlState,
        fatal: error.fatal,
        message: error.message
      });
      throw new Error(`Échec SQL : ${error.sql?.substring(0, 50)}...`);
    } finally {
      if (conn) await conn.end();
    }
  }

    /**
   * Valide l'existence du fichier SQL
   * @param {string} scriptPath 
   */
    async validateScriptFile(scriptPath) {
      try {
        await fs.access(scriptPath, fs.constants.R_OK);
        const stats = await fs.stat(scriptPath);
        console.log(`📄 Fichier SQL trouvé (${(stats.size / 1024).toFixed(2)} Ko)`);
      } catch (error) {
        throw new Error(`❌ Fichier SQL inaccessible: ${error.message}`);
      }
    }

  /**
   * Génère un mot de passe sécurisé.
   */
  generateSecurePassword() {
    const length = 14;
    const lowercase = 'abcdefghijklmnopqrstuvwxyz';
    const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const numbers = '0123456789';
    const specialChars = '@*$?!+-/';
    const allChars = lowercase + uppercase + numbers + specialChars;
  
    let password = '';
    password += lowercase[crypto.randomInt(lowercase.length)];
    password += uppercase[crypto.randomInt(uppercase.length)];
    password += numbers[crypto.randomInt(numbers.length)];
    password += specialChars[crypto.randomInt(specialChars.length)];
  
    for (let i = 4; i < length; i++) {
      password += allChars[crypto.randomInt(allChars.length)];
    }
  
    return password
      .split('')
      .sort(() => 0.5 - Math.random())
      .join('');
  }

  /**
   * Méthode de fermeture du service si besoin.
   */
  async close() {
    console.log("🛑 Fermeture du service DatabaseService");
  }
}

module.exports = new DatabaseService();
