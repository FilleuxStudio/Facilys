const { db } = require('../config/firebase-config'); // Assurez-vous que ce chemin est correct
const { v4: uuidv4 } = require('uuid');

class Product {
  constructor(data) {
    this.id = data.id || uuidv4(); 
    this.title = data.title;
    this.subtitle = data.subtitle;
    this.picture = data.picture;
    this.price = data.price;
    this.oldPrice = data.oldPrice;
    this.advice = data.advice;
    this.type = data.type;
    this.colorBagde = data.colorBagde;
    this.dateAdded = data.dateAdded;
  }

  // Méthode pour sauvegarder un product dans Firestore
  async save() {
    const productoRef = db.collection('products').doc(this.id); // Utiliser id comme identifiant unique
    return await productoRef.set({
      id: uuidv4(), 
      title: this.title,
      subtitle: this.subtitle,
      picture: this.picture,
      price: this.price,
      oldPrice: this.oldPrice,
      advice: this.advice,
      type: this.type,
      colorBagde: this.colorBagde,
      dateAdded: new Date().toISOString(),
    });
  }

  // Méthode pour récupérer un items
  static async findById(id) {
    const productoRef = db.collection('products').doc(id);
    const doc = await productoRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findAll() {
    try {
      const productsRef = db.collection('products');
      const snapshot = await productsRef.get();

      if (snapshot.empty) {
        console.log('Aucun produit trouvé.');
        return [];
      }

      return snapshot.docs.map(doc => ({
        id: doc.id,
        ...doc.data()
      }));
    } catch (error) {
      console.error('Erreur lors de la récupération des produits:', error);
      throw error;
    }
  }
}

module.exports = Product;