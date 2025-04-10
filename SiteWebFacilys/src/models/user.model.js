const { db } = require('../config/firestore'); // Assurez-vous que ce chemin est correct

class User {
  constructor(data) {
    this.companyName = data.companyName;
    this.logo = data.logo;
    this.firstName = data.firstName;
    this.lastName = data.lastName;
    this.email = data.email;
    this.siret = data.siret;
    this.addressclient = data.addressclient;
    this.phone = data.phone;
    this.password = data.password; 
    this.manager = data.manager;
    this.mariadbUser = data.mariadbUser;
    this.mariadbPassword = data.mariadbPassword;
    this.mariadbDb = data.mariadbDb;
  }

  // Méthode pour sauvegarder l'utilisateur dans Firestore
  async save() {
    const userRef = db.collection("users").doc(this.email); // Utiliser l'email comme identifiant unique
    return await userRef.set({
      companyName: this.companyName,
      logo: this.logo,
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      siret: this.siret,
      addressclient: this.addressclient,
      phone: this.phone,
      password: this.password, // Assurez-vous de hacher le mot de passe avant d'appeler cette méthode
      manager: this.manager,
      mariadbUser: this.mariadbUser,
      mariadbPassword: this.mariadbPassword,
      mariadbDb: this.mariadbDb,
    });
  }

  // Méthode pour récupérer un utilisateur par e-mail
 static async findByEmail(email) {
    const userRef = db.collection("users").doc(email);
    const doc = await userRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findAll() {
    try {
      const users = db.collection("users");
      const snapshot = await users.get();

      if (snapshot.empty) {
        console.log('Aucun client trouvé.');
        return [];
      }

      return snapshot.docs.map(doc => ({
        id: doc.id,
        ...doc.data()
      }));
    } catch (error) {
      console.error('Erreur lors de la récupération des client:', error);
      throw error;
    }
  }

  // Méthode pour mettre à jour un utilisateur dans Firestore
 static async update(email, updateData) {
    const userRef = db.collection("users").doc(email);

    try {
      await userRef.update(updateData);
      console.log("Document successfully updated");
      return true;
    } catch (error) {
      console.error("Error updating document: ", error);
      return false;
    }
  }

  async updateMariaDBInfo(email, dbInfo) {
    try {
      const updateData = {
        mariadbUser: dbInfo.dbUser,
        mariadbPassword: dbInfo.password,
        mariadbDb: dbInfo.dbName
      };

      const userRef = db.collection("users").doc(email);
      
      // Utilisation d'une transaction pour plus de sécurité
      await db.runTransaction(async (transaction) => {
        const doc = await transaction.get(userRef);
        if (!doc.exists) {
          throw new Error("Document utilisateur non trouvé");
        }
        
        transaction.update(userRef, updateData);
      });

      console.log("Informations MariaDB mises à jour avec succès");
      return true;
    } catch (error) {
      console.error("Erreur lors de la mise à jour MariaDB :", error);
      
      // Log supplémentaire pour le débogage
      if (error.code === 'NOT_FOUND') {
        console.error(`Utilisateur ${email} introuvable`);
      } else if (error.code === 'PERMISSION_DENIED') {
        console.error("Permissions Firestore insuffisantes");
      }
      
      throw new Error("Échec de la mise à jour des informations de base de données");
    }
  }
}

module.exports = User;