const mariadb = require("mariadb");
const fs = require("fs").promises;
const path = require("path");

// Configuration de connexion
const connectionConfig = {
  host: "node117-eu.n0c.com", // Remplacez par votre hostname
  user: "jmaqmsnt_user_ssss", // Votre utilisateur
  password: "ASDFrtegr85475498!!!", // Votre mot de passe
  database: "jmaqmsnt_user_ssss_db", // Nom de la base de données
  connectTimeout: 5000, // Timeout de connexion (en millisecondes)
  multipleStatements: true
};

/**
 * Teste la connexion à la base de données et exécute un script SQL.
 */
async function testConnection() {
  let conn;
  try {
    console.log("🔍 Test de connexion à la base de données...");
    conn = await mariadb.createConnection(connectionConfig);
    console.log("✅ Connexion réussie !");
    
    // Vérification de la version du serveur MariaDB
    const serverVersion = await conn.query("SELECT VERSION() AS version");
    console.log(`📌 Version MariaDB : ${serverVersion[0].version}`);
    
    // Validation du fichier SQL
    const scriptPath = path.join(__dirname, "../sql/", "init_db.sql");
    console.log(`📄 Chemin du fichier SQL : ${scriptPath}`);
    await validateScriptFile(scriptPath);

    // Lecture et exécution du script SQL
    const sqlScript = await fs.readFile(scriptPath, "utf8");
    console.log("🚀 Début de l'exécution du script SQL...");
    
    const startTime = Date.now();
    const res = await conn.query(sqlScript);
    const executionTime = ((Date.now() - startTime) / 1000).toFixed(2);

    console.log(`✅ Script exécuté avec succès en ${executionTime}s`);
    console.log(`ℹ️ Nombre d'opérations effectuées : ${res.affectedRows}`);
  } catch (error) {
    console.error("❌ ERREUR DÉTAILLÉE :", {
      code: error.code,
      sqlState: error.sqlState,
      fatal: error.fatal,
      message: error.message
    });
    throw new Error(`Échec SQL : ${error.sql?.substring(0, 50)}...`);
  } finally {
    if (conn) await conn.end(); // Ferme la connexion proprement
  }
}

/**
 * Valide l'existence du fichier SQL.
 * @param {string} scriptPath - Chemin vers le fichier SQL.
 */
async function validateScriptFile(scriptPath) {
  try {
    await fs.access(scriptPath, fs.constants.R_OK); // Vérifie si le fichier est accessible en lecture
    const stats = await fs.stat(scriptPath); // Récupère les informations sur le fichier
    console.log(`📄 Fichier SQL trouvé (${(stats.size / 1024).toFixed(2)} Ko)`);
  } catch (error) {
    throw new Error(`❌ Fichier SQL inaccessible : ${error.message}`);
  }
}

// Exécute le test
testConnection();
