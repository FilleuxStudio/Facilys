// routes/subscribe.js
const express = require('express');
const router = express.Router();
const nodemailer = require('nodemailer');

// Route POST pour traiter la soumission du formulaire
router.post('/', (req, res) => {
    const userEmail = req.body.email;

    // Configurer nodemailer pour utiliser le serveur SMTP de PlanetHoster
    const transporter = nodemailer.createTransport({
        host: 'mail.votre-domaine.com', // Remplacez par l'hôte SMTP de PlanetHoster
        port: 587,                      // Ou 465 si vous utilisez SSL
        secure: false,                  // false pour STARTTLS (recommandé pour le port 587)
        auth: {
            user: 'votre-adresse@votre-domaine.com', // Remplacez par votre adresse email
            pass: 'votre-mot-de-passe'               // Remplacez par le mot de passe de votre compte e-mail
        },
        tls: {
            rejectUnauthorized: false  // Option pour accepter les certificats auto-signés (si nécessaire)
        }
    });

    const mailOptions = {
        from: 'votre-adresse@votre-domaine.com',   // Adresse de l'expéditeur
        to: userEmail,                             // Adresse de l'utilisateur
        subject: 'Merci de vous être abonné',
        text: 'Merci de vous être abonné à notre newsletter !'
    };

    // Envoyer l'email
    transporter.sendMail(mailOptions, (error, info) => {
        if (error) {
            console.log(error);
            return res.status(500).send('Erreur lors de l\'envoi de l\'email');
        }
        console.log('Email envoyé: ' + info.response);
        res.send('Merci pour votre abonnement ! Un email de confirmation a été envoyé.');
    });
});

module.exports = router;
