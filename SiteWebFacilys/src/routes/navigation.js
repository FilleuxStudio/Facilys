const express = require('express');
const router = express.Router();


// Route pour la page "À propos de nous"
router.get('/about', (req, res) => {
    res.render('about'); // Affiche la page about.ejs
});

// Route pour la page "contact"
router.get('/contact', (req, res) => {
    res.render('contact'); 
});

// Route pour la page "privacy-policy"
router.get('/privacy-policy', (req, res) =>{
    res.render('privacy-policy');
});

module.exports = router;
