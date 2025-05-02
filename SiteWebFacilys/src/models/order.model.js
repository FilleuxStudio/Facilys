const { db } = require('../config/firestore'); // Assurez-vous que ce chemin est correct
const { v4: uuidv4 } = require('uuid');
const logger = require("../utils/logger");

class Order {
  constructor(data) {
    this.id = data.id || uuidv4(); 
    this.idProduct = data.idProduct;
    this.idUser = data.idUser;
    this.productName = data.productName;
    this.price = data.price;
    this.renewalDate = data.renewalDate;
    this.type = data.type;
    this.dateAdded = data.dateAdded;
  }

  // Méthode pour sauvegarder un product dans Firestore
  async save() {
    const ordertoRef = db.collection('orders').doc(this.id); // Utiliser id comme identifiant unique
    return await ordertoRef.set({
      id: this.id, 
      idProduct: this.idProduct,
      idUser: this.idUser,
      productName: this.productName,
      price: this.price,
      renewalDate: this.renewalDate,
      type: this.type,
      dateAdded: new Date().toISOString(),
    });
  }

  // Méthode pour récupérer un items
  static async findById(id) {
    const ordertoRef = db.collection('orders').doc(id);
    const doc = await ordertoRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findAll() {
    try {
      const ordertoRef = db.collection('orders');
      const snapshot = await ordertoRef.get();

      if (snapshot.empty) {
        console.log('Aucune commande trouvé.');
        logger.info("Aucune commande trouvé.");
        return [];
      }

      return snapshot.docs.map(doc => ({
        ...doc.data()
      }));
    } catch (error) {
      console.error('Erreur lors de la récupération des commandes:', error);
      logger.error('Erreur lors de la récupération des commandes:', error);
      throw error;
    }
  }

  static async findByUserId(idUser) {
    try {
      const ordertoRef = db.collection('orders').where('idUser', '==', idUser);
      const snapshot = await ordertoRef.get();

      if (snapshot.empty) {
        console.log(`Aucune commande trouvée pour l'utilisateur avec idUser: ${idUser}`);
        logger.info(`Aucune commande trouvée pour l'utilisateur avec idUser: ${idUser}`);
        return [];
      }

      // Retourne les données des commandes trouvées
      return snapshot.docs.map(doc => doc.data());
    } catch (error) {
      console.error('Erreur lors de la récupération des commandes par idUser:', error);
      logger.error('Erreur lors de la récupération des commandes par idUser:', erro);
      throw error;
    }
  }

   // Méthode pour mettre à jour un utilisateur dans Firestore
   static async update(id, updateData) {
    const ordertoRef = db.collection("orders").doc(id);

    try {
      await ordertoRef.update(updateData);
      console.log("Document successfully updated");
      logger.info("Document successfully updated");
      return true;
    } catch (error) {
      logger.error('Erreur lors de la récupération des commandes par idUser:', erro);
      console.error('Erreur lors de la récupération des commandes par idUser:', erro);
      return false;
    }
  }

   // Méthode pour supprimer un produit de Firestore
   static async delete(id) {
    const ordertoRef = db.collection('orders').doc(id);

    try {
      await ordertoRef.delete();
      console.log("Document successfully deleted");
      console.error("Document successfully deleted");
      return true;
    } catch (error) {
      console.error("Error deleting document: ", error);
      console.error("Error deleting document: ", error);
      return false;
    }
  }
}

module.exports = Order;