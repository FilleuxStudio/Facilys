const { db } = require('../config/firestore'); // Assurez-vous que ce chemin est correct
const { v4: uuidv4 } = require('uuid');
const logger = require("../utils/logger");

class Invoice {
  constructor(data) {
    this.id = data.id || uuidv4(); 
    this.idProduct = data.idProduct;
    this.idUser = data.idUser;
    this.idFacture = data.idFacture;
    this.price = data.price;
    this.date = data.date;
    this.status = data.status;
    this.dateAdded = data.dateAdded;
  }

  // Méthode pour sauvegarder un product dans Firestore
  async save() {
    const invoicetoRef = db.collection('invoice').doc(this.id); // Utiliser id comme identifiant unique
    return await invoicetoRef.set({
      id: this.id, 
      idProduct: this.idProduct,
      idUser: this.idUser,
      idFacture: this.idFacture,
      price: this.price,
      date: this.date,
      status: this.status,
      dateAdded: new Date().toISOString(),
    });
  }

  // Méthode pour récupérer un items
  static async findById(id) {
    const invoicetoRef = db.collection('invoice').doc(id);
    const doc = await invoicetoRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findAll() {
    try {
      const invoicetoRef = db.collection('invoice');
      const snapshot = await invoicetoRef.get();

      if (snapshot.empty) {
        console.log('Aucune facture trouvé.');
        return [];
      }

      return snapshot.docs.map(doc => ({
        ...doc.data()
      }));
    } catch (error) {
      console.error('Erreur lors de la récupération des factures:', error);
      logger.error('Erreur lors de la récupération des factures:', error);
      throw error;
    }
  }

  static async findByUserId(idUser) {
    try {
      const invoicetoRef = db.collection('invoice').where('idUser', '==', idUser);
      const snapshot = await invoicetoRef.get();

      if (snapshot.empty) {
        console.log(`Aucune factures trouvée pour l'utilisateur avec idUser: ${idUser}`);
        return [];
      }

      // Retourne les données des commandes trouvées
      return snapshot.docs.map(doc => doc.data());
    } catch (error) {
      console.error('Erreur lors de la récupération des factures par idUser:', error);
      logger.error('Erreur lors de la récupération des factures par idUser:', error);
      throw error;
    }
  }

   // Méthode pour mettre à jour un utilisateur dans Firestore
   static async update(id, updateData) {
    const invoicetoRef = db.collection("invoice").doc(id);

    try {
      await invoicetoRef.update(updateData);
      console.log("Document successfully updated");
      logger.info("Document successfully updated");
      return true;
    } catch (error) {
      console.error("Error updating document: ", error);
      logger.error("Error updating document: ", error);
      return false;
    }
  }

   // Méthode pour supprimer un produit de Firestore
   static async delete(id) {
    const invoicetoRef = db.collection('invoice').doc(id);

    try {
      await invoicetoRef.delete();
      console.log("Document successfully deleted");
      logger.info("Document successfully deleted");
      return true;
    } catch (error) {
      console.error("Error deleting document: ", error);
      logger.error("Error deleting document: ", error);
      return false;
    }
  }
}

module.exports = Invoice;