const { db } = require('../config/firebase-config'); // Assurez-vous que ce chemin est correct
const { v4: uuidv4 } = require('uuid');

class Team {
  constructor(data) {
    this.id = data.id || uuidv4(); 
    this.fname = data.fname;
    this.lname = data.lname;
    this.type = data.type;
    this.password = data.password;
    this.team = data.team;
    this.manager = data.manager;
    this.email = data.email;
    this.dateAdded = data.dateAdded;
  }

  // Méthode pour sauvegarder un product dans Firestore
  async save() {
    const teamtoRef = db.collection('teams').doc(this.id); // Utiliser id comme identifiant unique
    return await teamtoRef.set({
      id: this.id, 
      fname:this.fname,
      lname:this.lname,
      type:this.type,
      password:this.password,
      team:this.team,
      manager:this.manager,
      email:this.email,
      dateAdded: new Date().toISOString(),
    });
  }

  // Méthode pour récupérer un items
  static async findById(id) {
    const teamtoRef = db.collection('teams').doc(id);
    const doc = await teamtoRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findByUserId(idUser) {
    try {
      const teamtoRef = db.collection('teams').where('manager', '==', idUser);
      const snapshot = await teamtoRef.get();

      if (snapshot.empty) {
        console.log(`Aucun collaborateur trouvée pour l'utilisateur avec idUser: ${idUser}`);
        return [];
      }

      // Retourne les données des commandes trouvées
      return snapshot.docs.map(doc => doc.data());
    } catch (error) {
      console.error('Erreur lors de la récupération des collaborateur par idUser:', error);
      throw error;
    }
  }

  static async findAll() {
    try {
      const teamRef = db.collection('teams');
      const snapshot = await teamRef.get();

      if (snapshot.empty) {
        console.log('Aucune team trouvé.');
        return [];
      }

      return snapshot.docs.map(doc => ({
        ...doc.data()
      }));
    } catch (error) {
      console.error('Erreur lors de la récupération de la team:', error);
      throw error;
    }
  }

   // Méthode pour mettre à jour un utilisateur dans Firestore
   static async update(id, updateData) {
    const teamRef = db.collection("teams").doc(id);

    try {
      await teamRef.update(updateData);
      console.log("Document successfully updated");
      return true;
    } catch (error) {
      console.error("Error updating document: ", error);
      return false;
    }
  }

   // Méthode pour supprimer un produit de Firestore
   static async delete(id) {
    const productRef = db.collection('teams').doc(id);

    try {
      await productRef.delete();
      console.log("Document successfully deleted");
      return true;
    } catch (error) {
      console.error("Error deleting document: ", error);
      return false;
    }
  }
}

module.exports = Team;