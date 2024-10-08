const { db } = require('../config/firebase-config'); // Assurez-vous que ce chemin est correct

class Product {
  constructor(data) {
    this.id = data.id;
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
    const productoRef = db.collection('product').doc(this.id); // Utiliser id comme identifiant unique
    return await productoRef.set({
      id: this.id,
      title: this.title,
      subtitle: this.subtitle,
      picture: this.picture,
      price: this.price,
      oldPrice: this.oldPrice,
      advice: this.advice,
      type: this.type,
      colorBagde: this.colorBagde,
      dateAdded: this.dateAdded,
    });
  }

  // Méthode pour récupérer un items
  static async findById(id) {
    const productoRef = db.collection('product').doc(id);
    const doc = await productoRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }

  static async findAll() {
    const db = getFirestore();
    const productsRef = collection(db, 'product');
    const snapshot = await getDocs(productsRef);
    
    return snapshot.docs.map(doc => ({
      id: doc.id,
      ...doc.data()
    }));
  }
}

module.exports = Product;