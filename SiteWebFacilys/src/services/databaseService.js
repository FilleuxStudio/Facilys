const mariadb = require('mariadb');
const fs = require('fs').promises;
const path = require('path');
const crypto = require('crypto');

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

      // Échappement des éléments dynamiques
      const dbNameEscaped = conn.escapeId(dbName);
      const userEscaped = conn.escape(mariadbUser);
      const passwordEscaped = conn.escape(mariadbPassword);

      await conn.query(`CREATE DATABASE IF NOT EXISTS ${dbNameEscaped}`);
      await conn.query(`CREATE USER ${userEscaped}@'%' IDENTIFIED BY ${passwordEscaped}`);
      await conn.query(`GRANT ALL PRIVILEGES ON ${dbNameEscaped}.* TO ${userEscaped}@'%'`);
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
    const scriptPath = path.join(__dirname, '..', 'sql', 'init_db.sql');
    const sqlScript = await fs.readFile(scriptPath, 'utf8');

    await conn.query(`USE ${conn.escapeId(dbName)}`);
    const statements = sqlScript.split(/;\s*(?=CREATE|INSERT|ALTER|DROP|UPDATE|DELETE|SELECT)/i)
                               .filter(statement => statement.trim() !== '');
    
    for (const statement of statements) {
      if (statement.trim()) {
        await conn.query(statement);
      }
    }
  }

  generateSecurePassword() {
    return crypto.randomBytes(16).toString('hex');
  }

  async close() {
    await this.pool.end();
  }
}

module.exports = new DatabaseService();