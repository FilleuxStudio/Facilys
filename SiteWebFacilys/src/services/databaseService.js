const mariadb = require('mariadb');
const fs = require('fs').promises;
const path = require('path');

class DatabaseService {
  constructor() {
    this.pool = mariadb.createPool({
      host: process.env.DB_HOST,
      user: process.env.DB_USER,
      password: process.env.DB_PASSWORD,
      connectionLimit: 5
    });
  }

  async getConnection() {
    return await this.pool.getConnection();
  }

  async createUserDatabase(userId) {
    let conn;
    try {
      conn = await this.getConnection();
      const dbName = `user_${userId}_db`;
      const mariadbUser = `user_${userId}`;
      const mariadbPassword = this.generateSecurePassword();

      await conn.query(`CREATE DATABASE IF NOT EXISTS ${dbName}`);
      await conn.query(`CREATE USER '${mariadbUser}'@'%' IDENTIFIED BY '${mariadbPassword}'`);
      await conn.query(`GRANT ALL PRIVILEGES ON ${dbName}.* TO '${mariadbUser}'@'%'`);
      await conn.query('FLUSH PRIVILEGES');

      await this.executeSQLScript(conn, dbName);

      return {
        name: dbName,
        user: mariadbUser,
        password: mariadbPassword
      };
    } catch (error) {
      console.error('Erreur lors de la création de la base de données utilisateur:', error);
      throw error;
    } finally {
      if (conn) conn.release();
    }
  }

  async executeSQLScript(conn, dbName) {
    const scriptPath = path.join(__dirname, '..', 'sql', 'init_user_db.sql');
    const sqlScript = await fs.readFile(scriptPath, 'utf8');

    await conn.query(`USE ${dbName}`);
    const statements = sqlScript.split(';').filter(statement => statement.trim() !== '');
    for (let statement of statements) {
      await conn.query(statement);
    }
  }

  generateSecurePassword() {
    // Implémentez la logique pour générer un mot de passe sécurisé
    return 'password_sécurisé'; // À remplacer par une vraie implémentation
  }

  async close() {
    await this.pool.end();
  }
}

module.exports = new DatabaseService();
