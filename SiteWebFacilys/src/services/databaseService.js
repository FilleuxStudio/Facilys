const mariadb = require("mariadb");
const axiosLib = require("axios");
const fs = require("fs").promises;
const path = require("path");
const crypto = require("crypto");

class DatabaseService {
  constructor() {
    // Vérification des variables d'environnement
    const { PLANETHOSTER_API_KEY, PLANETHOSTER_API_USER, WORLDACCOUNTS } = process.env;
    if (!PLANETHOSTER_API_KEY || !PLANETHOSTER_API_USER || !WORLDACCOUNTS) {
      throw new Error("Les variables d'environnement pour l'API PlanetHoster ne sont pas définies.");
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

        if (response.data.some(db => db.name === databaseName)) {
          console.log(`✅ Base de données "${databaseName}" confirmée.`);
          return;
        }
      } catch (error) {
        console.error("Erreur lors de la récupération des bases de données :", error);
      }

      console.log(`⏳ En attente de la base "${databaseName}"... (${i + 1}/${retries})`);
      await new Promise(res => setTimeout(res, delay));
    }
    throw new Error(`⛔ Timeout : La base "${databaseName}" n'a pas été trouvée après plusieurs tentatives.`);
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

        if (response.data.includes(dbUser)) {
          console.log(`✅ Utilisateur "${dbUser}" confirmé.`);
          return;
        }
      } catch (error) {
        console.error("Erreur lors de la récupération des utilisateurs :", error);
      }

      console.log(`⏳ En attente de l'utilisateur "${dbUser}"... (${i + 1}/${retries})`);
      await new Promise(res => setTimeout(res, delay));
    }
    throw new Error(`⛔ Timeout : L'utilisateur "${dbUser}" n'a pas été trouvé après plusieurs tentatives.`);
  }

  /**
   * Crée une base de données, un utilisateur et leur attribue les privilèges nécessaires.
   */
  async createDatabase(userId) {
    const dbName = `user_${userId}_db`;
    const dbUser = `user_${userId}`;
    const dbPassword = this.generateSecurePassword();

    const prefixedDbName = `${this.suffix}${dbName}`;
    const prefixedDbUser = `${this.suffix}${dbUser}`;

    try {
      // 1️⃣ Création de la base de données
      console.log(`🚀 Création de la base de données "${dbName}"...`);
      await this.apiClient.post("/hosting/database", {
        id: this.worldAccountId,
        name: dbName,
        databaseType: "MYSQL",
      });

      await this.waitForDatabaseCreation(prefixedDbName);

      // 2️⃣ Création de l'utilisateur
      console.log(`🚀 Création de l'utilisateur "${dbUser}"...`);
      await this.apiClient.post("/hosting/database/user", {
        id: this.worldAccountId,
        dbUser: dbUser,
        password: dbPassword,
        databaseType: "MYSQL",
      });

      await this.waitForUserCreation(prefixedDbUser);

      // 3️⃣ Attribution des privilèges à l'utilisateur sur la base
      console.log(`🚀 Attribution des privilèges à "${dbUser}" sur "${dbName}"...`);
      await this.apiClient.put("/hosting/database/user/privileges", {
        databaseType: "MYSQL",
        privileges: "ALL PRIVILEGES",
        id: this.worldAccountId,
        databaseName: prefixedDbName,
        databaseUsername: prefixedDbUser,
      });

      console.log(`✅ Privilèges accordés avec succès.`);

      // 4️⃣ Exécution du script SQL d'initialisation
      await this.executeSQLScript({
        host: "node117-eu.n0c.com",
        user: prefixedDbUser,
        password: dbPassword,
        database: prefixedDbName,
      });

      return { dbUser: prefixedDbUser, dbName: prefixedDbName, password: dbPassword };
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
      console.log(`🚀 Exécution du script SQL pour "${connectionConfig.database}"...`);

      const scriptPath = path.join(__dirname, "..", "sql", "init_db.sql");
      const sqlScript = await fs.readFile(scriptPath, "utf8");

      conn = await mariadb.createConnection(connectionConfig);

      const statements = sqlScript
        .split(/;\s*(?=(CREATE|INSERT|ALTER|DROP|UPDATE|DELETE|SELECT))/i)
        .map(stmt => stmt.trim())
        .filter(stmt => stmt.length > 0);

      for (const statement of statements) {
        await conn.query(statement);
      }

      console.log(`✅ Script SQL exécuté avec succès sur "${connectionConfig.database}".`);
    } catch (error) {
      console.error("❌ Erreur lors de l'exécution du script SQL :", error);
      throw error;
    } finally {
      if (conn) await conn.end();
    }
  }

  /**
   * Génère un mot de passe sécurisé.
   */
  generateSecurePassword() {
    return crypto.randomBytes(16).toString("hex");
  }

  /**
   * Méthode de fermeture du service si besoin.
   */
  async close() {
    console.log("🛑 Fermeture du service DatabaseService");
  }
}

module.exports = new DatabaseService();
