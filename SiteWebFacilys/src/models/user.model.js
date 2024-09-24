const { db } = require('../config/firebase-config'); // Assurez-vous que ce chemin est correct

class User {
  constructor(data) {
    this.companyName = data.companyName;
    this.firstName = data.firstName;
    this.lastName = data.lastName;
    this.email = data.email;
    this.password = data.password; // Note: Le mot de passe doit être haché avant d'être stocké
    this.manager = data.manager;
  }

  // Méthode pour sauvegarder l'utilisateur dans Firestore
  async save() {
    const userRef = db.collection('users').doc(this.email); // Utiliser l'email comme identifiant unique
    return await userRef.set({
      companyName: this.companyName,
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password, // Assurez-vous de hacher le mot de passe avant d'appeler cette méthode
      manager: this.manager,
    });
  }

  // Méthode pour récupérer un utilisateur par e-mail
  static async findByEmail(email) {
    const userRef = db.collection('users').doc(email);
    const doc = await userRef.get();

    if (!doc.exists) {
      return null;
    }
    return doc.data();
  }
}

module.exports = User;