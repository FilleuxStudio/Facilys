const mariadb = require('mariadb');
const schedule = require('node-schedule');
const logger = require("../utils/logger");

class ConnectionPoolService {
  constructor() {
    this.pools = new Map();
  }

  getPool(user) {
    if (!this.pools.has(user.email)) {
      const pool = mariadb.createPool({
        host: 'localhost',
        user: user.mariadbUser,
        password: user.mariadbPassword,
        database: user.mariadbDb,
        connectionLimit: 100, // Limite de connexions simultanées
        acquireTimeout: 5000,
      });

      this.pools.set(user.email, pool);
    }

    return this.pools.get(user.email);
  }

  async executeQuery(user, query) {
    console.log(user);
    const pool = this.getPool(user);
    let conn;
    try {
      conn = await pool.getConnection();
      
      const result = await conn.query(query); 
  
      return result;
    } catch (err) {
      console.error("Erreur lors de l'exécution de la requête :", err);
      logger.error("Erreur lors de l'exécution de la requête :", err);
      throw err;
    } finally {
      if (conn) conn.release();
    }
  }
  

  schedulePoolClosure() {
    schedule.scheduleJob('30 1 * * *', async () => {
      console.log('Fermeture programmée des pools de connexions');
      logger.info('Fermeture programmée des pools de connexions');
      await this.close();
      console.log('Tous les pools de connexions ont été fermés');
      logger.error('Tous les pools de connexions ont été fermés');
    });
  }

  async close() {
    for (const pool of this.pools.values()) {
      try {
        await pool.end(); // Ferme proprement chaque pool
      } catch (e) {
        console.warn("Erreur lors de la fermeture d'un pool :", e);
        logger.error("Erreur lors de la fermeture d'un pool :", e);
      }
    }
    this.pools.clear();
  }
}

module.exports = new ConnectionPoolService();