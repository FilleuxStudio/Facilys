const mariadb = require("mariadb");
const axiosLib = require("axios");
const fs = require("fs").promises;
const path = require("path");
const crypto = require("crypto");

class DatabaseService {
  constructor() {
    // Destructuration et vérification des variables d'environnement
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

    // Création d'une instance Axios configurée
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
   * Crée une base de données et un utilisateur associé avec tous les privilèges,
   * puis exécute un script SQL d'initialisation.
   * @param {string|number} userId - L'identifiant de l'utilisateur dans votre application.
   * @returns {Promise<Object>} - Objet contenant le nom de la DB, le nom d'utilisateur et le mot de passe généré.
   */
  async createDatabase(userId) {
    // Génération des identifiants et mot de passe
    const dbName = `user_${userId}_db`;
    const dbUser = `user_${userId}`;
    const dbPassword = this.generateSecurePassword();

    // Création des noms avec préfixe
    const prefixedDbName = `${this.suffix}${dbName}`;
    const prefixedDbUser = `${this.suffix}${dbUser}`;

    try {
      // 1. Création de la base de données
      const createDbPayload = {
        id: this.worldAccountId,
        name: dbName,
        databaseType: "MYSQL",
      };
      const dbResponse = await this.apiClient.post(
        "/hosting/database",
        createDbPayload
      );
      if (![200, 201].includes(dbResponse.status)) {
        throw new Error(
          `Erreur lors de la création de la DB : ${dbResponse.data.message}`
        );
      }
      console.log(`Base de données "${dbName}" créée via l'API PlanetHoster.`);

      // 1.5. Récupération des bases de données existantes
      const getDatabasesPayload = {
        id: this.worldAccountId,
        databaseType: "MYSQL",
      };
      const dbListResponse = await this.apiClient.get("/hosting/databases", {
        params: getDatabasesPayload,
      });
      if (![200, 201].includes(dbListResponse.status)) {
        throw new Error(
          `Erreur lors de la récupération des bases de données : ${dbListResponse.data.message}`
        );
      }
      console.log("Bases de données existantes : ", dbListResponse.data);

      // Recherche de la base de données créée
      const existingDb = dbListResponse.data.find(
        (db) => db.name === prefixedDbName
      );
      if (!existingDb) {
        throw new Error(
          `La base de données "${dbName}" n'existe pas dans la liste.`
        );
      }
      console.log(`Base de données "${existingDb.name}" trouvée.`);

      // 2. Création de l'utilisateur associé
      const createUserPayload = {
        id: this.worldAccountId,
        dbUser: dbUser,
        password: dbPassword,
        databaseType: "MYSQL",
      };
      const userResponse = await this.apiClient.post(
        "/hosting/database/user",
        createUserPayload
      );
      if (![200, 201].includes(userResponse.status)) {
        throw new Error(
          `Erreur lors de la création de l'utilisateur : ${userResponse.data.message}`
        );
      }
      console.log(`Utilisateur "${dbUser}" créé via l'API PlanetHoster.`);

      // 2.5. Récupération des utilisateurs existants
      const getUsersPayload = {
        id: this.worldAccountId,
        databaseType: "MYSQL",
      };
      const userListResponse = await this.apiClient.get(
        "/hosting/databases/users",
        { params: getUsersPayload }
      );
      if (![200, 201].includes(userListResponse.status)) {
        throw new Error(
          `Erreur lors de la récupération des utilisateurs : ${userListResponse.data.message}`
        );
      }
      console.log("Utilisateurs existants : ", userListResponse.data);

      // Aplatir la liste si nécessaire et recherche de l'utilisateur
      const allUsers = Array.isArray(userListResponse.data)
        ? userListResponse.data.flat()
        : [];
      const existingUser = allUsers.find((user) => user === prefixedDbUser);
      if (!existingUser) {
        throw new Error(
          `L'utilisateur "${prefixedDbUser}" n'existe pas dans la liste.`
        );
      }

      // 3. Attribution des privilèges à l'utilisateur sur la base
      const grantPayload = {
        databaseType: "MYSQL",
        privileges: "ALL PRIVILEGES",
        id: this.worldAccountId,
        databaseName: prefixedDbName,
        databaseUsername: prefixedDbUser,
      };
      const grantResponse = await this.apiClient.put(
        "/hosting/database/user/privileges",
        grantPayload
      );
      if (![200, 201].includes(grantResponse.status)) {
        throw new Error(
          `Erreur lors de l'attribution des privilèges : ${grantResponse.data.message}`
        );
      }
      console.log(
        `Privilèges "ALL PRIVILEGES" accordés à "${dbUser}" sur "${dbName}".`
      );

      // 4. Exécution du script SQL d'initialisation
      await this.executeSQLScript({
        host: "node117-eu.n0c.com", // Hôte du serveur MariaDB/MySQL
        user: prefixedDbUser,
        password: dbPassword,
        database: prefixedDbName,
      });

      return {
        dbUser: prefixedDbUser,
        dbName: prefixedDbName,
        password: dbPassword,
      };
    } catch (error) {
      console.error("Erreur dans createDatabase :", error);
      throw error;
    }
  }

  /**
   * Exécute un script SQL pour initialiser la base de données.
   * @param {Object} connectionConfig - La configuration de connexion {host, user, password, database}.
   */
  async executeSQLScript(connectionConfig) {
    let conn;
    try {
      // Chargement du script SQL depuis le fichier
      const scriptPath = path.join(__dirname, "..", "sql", "init_db.sql");
      const sqlScript = await fs.readFile(scriptPath, "utf8");

      // Établir la connexion à la base de données
      conn = await mariadb.createConnection(connectionConfig);

      // Découper le script en instructions individuelles
      const statements = sqlScript
        .split(/;\s*(?=(CREATE|INSERT|ALTER|DROP|UPDATE|DELETE|SELECT))/i)
        .map((stmt) => stmt.trim())
        .filter((stmt) => stmt.length > 0);

      // Exécution de chaque instruction SQL
      for (const statement of statements) {
        await conn.query(statement);
      }
      console.log(
        `Script SQL exécuté sur la base "${connectionConfig.database}".`
      );
    } catch (error) {
      console.error("Erreur lors de l'exécution du script SQL :", error);
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
    return crypto.randomBytes(16).toString("hex");
  }

  /**
   * Méthode de fermeture du service si besoin.
   */
  async close() {
    console.log("Fermeture du service DatabaseService");
  }
}

module.exports = new DatabaseService();
