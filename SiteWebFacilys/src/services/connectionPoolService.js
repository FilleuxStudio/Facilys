const mariadb = require('mariadb');
const schedule = require('node-schedule');
const User = require('../models/user.model'); // Assurez-vous que le chemin est correct

class ConnectionPoolService {
  constructor() {
    this.pools = new Map();
  }

  async getPool(user) {
    if (!this.pools.has(user.email)) {
      const pool = mariadb.createPool({
        host: process.env.DB_HOST,
        user: user.mariadbUser,
        password: user.mariadbPassword,
        database: user.mariadbDb,
        connectionLimit: 5
      });
      this.pools.set(user.email, pool);
    }
    return this.pools.get(user.email);
  }

  async executeQuery(user, query, params = []) {
    const pool = await this.getPool(user);
    let conn;
    try {
      conn = await pool.getConnection();
      return await conn.query(query, params);
    } finally {
      if (conn) conn.release();
    }
  }

  schedulePoolClosure() {
    schedule.scheduleJob('30 1 * * *', async () => {
      console.log('Fermeture programmée des pools de connexions');
      await this.close();
      console.log('Tous les pools de connexions ont été fermés');
    });
  }

  async close() {
    for (const pool of this.pools.values()) {
      await pool.end();
    }
    this.pools.clear();
  }
}

module.exports = new ConnectionPoolService();
